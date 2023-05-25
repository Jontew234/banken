using Platinum.Areas.Identity.Data;
using Platinum.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using System.Transactions;

namespace Platinum.Controllers
{
    [Authorize]
    public class LoanController : Controller
    {
        private readonly UserManager<Customer> _userManager;
        private readonly MinDbContext _context;
        private readonly TransactionController _tran;

        public LoanController(UserManager<Customer> userManager, MinDbContext context, TransactionController tran)
        {
            _context = context;
            _userManager = userManager;
            _tran = tran;
        }
        public async Task<IActionResult> Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(CheckLoanTermsViewModel model)
        {

            if (ModelState.IsValid)
            {
                Customer user = await _userManager.GetUserAsync(User);
                // kanske ej korrekt vid investmentaccounts
                decimal UserBalance = _context.bankAccounts.Where(a => !(a is BusinessAccount)).Sum(a => a.Balance);

                LoanTerms LoanTerm = await GetInterestRate(UserBalance, model.Amount);
               
                if (LoanTerm is not null)
                {
                    decimal YearlyRent = await RentYear(LoanTerm.Interestrate, (decimal)model.Amount);
                    CheckLoanTermsViewModel mod = new CheckLoanTermsViewModel
                    {
                        Amount = model.Amount,
                        Interest = YearlyRent,
                        InterestRate = LoanTerm.Interestrate,
                        UserName = user.Id,
                        Months = model.Months,
                        Accounts = await _context.bankAccounts .Where(a => a.CustomerId == user.Id && !(a is BusinessAccount)).ToListAsync(),
                        loanTerm = LoanTerm.Id

                    };
                    return View(mod);
                }
                // hur mycket får personen låna
                else
                {
                    decimal MaxLoan = await GetMaxLoan(UserBalance);
                    // hans saldo / sista räntenivån isch
                    ViewBag.Denied = "You are not allowed to borrow this amount. The max amount to borrow is " + MaxLoan.ToString();
                    return View(model);
                }
            }
            // behöver displaya snygga errors
            else
            {
                List<string> ErrorList = new List<string>();
                if (model.Amount == null || model.Amount == 0)
                {
                    ErrorList.Add(" amount has wrong format");
                }
                if (model.Months == null || model.Months == 0)
                {
                    ErrorList.Add("Months has wrong format");
                }
                ViewBag.Errors = ErrorList;

                return View();
            }


        }

        private async Task<LoanTerms> GetInterestRate(decimal sum, decimal loanAmount)
        {

            Customer cus = await _userManager.GetUserAsync(User);
            decimal previousLoans = _context.loans.Where(a => a.CustomerId == cus.Id).Sum(a => a.LeftToPay);
            decimal balanceAfterLoan = sum - previousLoans;

            decimal percent = loanAmount / balanceAfterLoan;
            List<LoanTerms> terms = await _context.LoanTerms.Where(a => a.IsActive == true).OrderBy(a => a.Terms).ToListAsync();
            if(terms is not null)
            {
                for (int i = 0; i < terms.Count; i++)
                {
                    LoanTerms loanTerms = terms[i];
                    // vid true så sätt den Loantermen
                    if (percent < loanTerms.Terms)
                    {
                        return loanTerms;
                        // denna term ska diplayas iaf
                    }
                }
            }
           
            return null;

        }

        private async Task<decimal> RentYear(decimal interestrate, decimal amount)
        {

            decimal rent = (0.01m * interestrate) * amount;
            return rent;
        }

        [HttpPost]
        // registrera lånet i databasen!
        public async Task<IActionResult> GetLoan(CheckLoanTermsViewModel model)
        {
            if (ModelState.IsValid)
            {
                Loan newLoan = new Loan();
                BusinessAccount to = await _context.bankAccounts.OfType<BusinessAccount>().FirstOrDefaultAsync();
                if (to == null) 
                {
                    return BadRequest();
                }
               int toId = to.Id;
                newLoan.Amount = model.Amount;
                newLoan.Months = (int)model.Months;
                newLoan.InterestRate = (decimal)model.InterestRate;
                newLoan.EndDate = DateTime.Now.AddMonths((int)model.Months);
                newLoan.CustomerId = _userManager.GetUserId(User);
                newLoan.LeftToPay = model.Amount;
                // byt till bankdirektörens konto
                newLoan.ToAccount = to;
                newLoan.AccountNumber = model.ChoosenAccount;
                    newLoan.LoanTermsId = model.loanTerm;
                newLoan.BankAccountId = await _context.bankAccounts.Where(a => a.AccountNumber == model.ChoosenAccount).Select(a => a.Id).FirstOrDefaultAsync();
                newLoan.BankAccount = await _context.bankAccounts.Where(a => a.AccountNumber == model.ChoosenAccount).FirstOrDefaultAsync();
                // lägg in where user = bankdirektören eller liknande
                newLoan.BankAccIdTo = toId;
                newLoan.Terms = await _context.LoanTerms.Where(a => a.Id == model.loanTerm).FirstOrDefaultAsync();
                newLoan.User = await _userManager.GetUserAsync(User);


                

                using (var transactions = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {

                        await _context.loans.AddAsync(newLoan);
                        await _context.SaveChangesAsync();
                        await _tran.LoanTransaction(model.ChoosenAccount, (decimal)model.Amount);
                        await _tran.TransferMoney(model.ChoosenAccount, (decimal)model.Amount);

                        await transactions.CommitAsync();
                    }
                    catch (Exception ex)

                    {
                        await transactions.RollbackAsync();
                        return View("Error");

                    }
                }
                // En vy och måste skapa en sammanställning att skicka med
                // summa ränta i kr och i % slutdatum kontonummer
                LoanSummaryViewModel returnModel = new()
                {
                    Amount = model.Amount,
                    EndDate = newLoan.EndDate,
                    AccountNumber = newLoan.AccountNumber,
                    YearlyInterest = await RentYear((decimal)model.InterestRate, model.Amount),
                    Interestrate = (decimal)model.InterestRate

                };
                return RedirectToAction("LoanSummary", returnModel);
            }
            else
            {
                return View();
            }
        }

        private async Task<decimal> GetMaxLoan(decimal UserBalance)
        {
            List<LoanTerms> terms = await _context.LoanTerms.Where(a => a.IsActive == true).OrderBy(a => a.Terms).ToListAsync();

            LoanTerms answer = terms[terms.Count - 1];
            // de du vill låna ska får max vara 50%
            decimal MaxLoan = answer.Terms * UserBalance;
            return MaxLoan;
        }

        public async Task<IActionResult> LoanSummary(LoanSummaryViewModel model)
        {

            return View(model);
        }

        public async Task<IActionResult> Alloan()
        {
            if (TempData["Error"] is not null)
            {
                ViewBag.Error = TempData["Error"];
            }
            if (TempData["Success"] is not null)
            {
                ViewBag.Success = TempData["Success"];
            }

            string id = _userManager.GetUserId(User);
            var alloans = await _context.loans.Where(a => a.CustomerId == id).ToListAsync();

            AlloansViewModel model = new AlloansViewModel();
            model.LoanSummary = new List<LoanSummaryViewModel>();
            foreach (var loan in alloans)
            {
                LoanSummaryViewModel l = new()
                {
                    AccountNumber = loan.AccountNumber,
                    Amount = loan.Amount,
                    EndDate = loan.EndDate,
                    Interestrate = loan.InterestRate,
                    LeftToPay = loan.LeftToPay,
                    Id = loan.ID,
                    Accounts = await _context.bankAccounts
                   .Where(a => a.CustomerId == id && !(a is InvestmentAccount || a is BusinessAccount))
                   .ToListAsync()


                };
                model.LoanSummary.Add(l);
            }
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> MakePayment(int Id, decimal PaymentAmount, int selectedAccountId, AlloansViewModel alloansViewModel)
        {
            BankAccount account = await _context.bankAccounts.Where(a => a.Id == selectedAccountId).FirstOrDefaultAsync();

            if (PaymentAmount < 0 || PaymentAmount == 0)
            {
                TempData["Error"] = "Not a valid amount!!";
            }
            else if (PaymentAmount > account.Balance)
            {
                TempData["Error"] = "You dont have enough money";

                return RedirectToAction("Alloan");
            }

            else
            {
                Loan loan = await _context.loans.Where(a => a.ID == Id).FirstOrDefaultAsync();
                if (loan.LeftToPay == 0)
                {
                    TempData["Error"] = "Loan already repaid";
                    return RedirectToAction("Alloan");
                }
                bool check = false;
                decimal amount = 0;
                if ((loan.LeftToPay - PaymentAmount) < 0)
                {
                    account.Balance -= loan.LeftToPay;
                    amount = loan.LeftToPay;
                    loan.LeftToPay = 0;
                    check = true;

                }
                else
                {
                    amount = PaymentAmount;
                    loan.LeftToPay -= PaymentAmount;
                    account.Balance -= PaymentAmount;
                    check = true;

                }

                if (check)
                {
                    // logga det som transaktrion
                    BankAccount to = await _context.bankAccounts.Where(a => a.Id == loan.BankAccIdTo).FirstOrDefaultAsync();
                    Tran tran = new()
                    {
                        Amount = amount,
                        Date = DateTime.Now,
                        FromBankAccount = account,
                        ToBankAccount = to ,
                        FromBankAccountId = account.Id,
                        ToBankAccountId = to.Id,
                        Category = "Amortisation"


                    };

                    await _context.transactions.AddAsync(tran);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Payment succeded";
                    // redirecta till en sammanställning här??
                }
                
            }
   

            return RedirectToAction("Alloan");

        }

      
    }
   
}
