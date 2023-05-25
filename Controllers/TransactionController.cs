using Platinum.Areas.Identity.Data;
using Platinum.Models;
using iText.Commons.Actions.Contexts;
using iText.Layout.Splitting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Dependency;
using System.Collections.Generic;
using System.Security.Cryptography.Xml;
using System.Security.Principal;
using System.Transactions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Platinum.Controllers
{
    [Authorize]
    public class TransactionController : Controller
    {
        private readonly UserManager<Customer> _userManager;
        private readonly MinDbContext _context;
        private readonly InvoiceController _invoiceController;

        public TransactionController(MinDbContext context, UserManager<Customer> userManager, InvoiceController invoiceController)
        {

            _userManager = userManager;
            _context = context;
            _invoiceController = invoiceController;
        }
      
        [HttpGet]
        public async  Task<IActionResult> Index()
        {
            string errorMessage = HttpContext.Session.GetString("ErrorMessage");
            string successMessage = HttpContext.Session.GetString("succesMessage");

            if (!string.IsNullOrEmpty(errorMessage))
            {
                // Display the error message in the view
                ViewBag.ErrorMessage = errorMessage;

                // Remove the error message from the session so it's not displayed again
                HttpContext.Session.Remove("ErrorMessage");
            }


            if (!string.IsNullOrEmpty(successMessage))
            {
                // Display the error message in the view
                ViewBag.successMessage = successMessage;

                // Remove the error message from the session so it's not displayed again
                HttpContext.Session.Remove("succesMessage");
            }
            DisplayAllAccountsViewModel Display = new DisplayAllAccountsViewModel();
            Display.Accounts = new List<AccountViewModel>();
            List<BankAccount> allAccounts = new List<BankAccount>();
            allAccounts =  await _context.bankAccounts.Where(a => !(a is BusinessAccount)).ToListAsync();

            foreach (var account in allAccounts) {
                AccountViewModel acc = new AccountViewModel();
                acc.AccountNumber = account.AccountNumber;
                acc.Id = account.Id;
                acc.AccountType = account.AccountType;
                
                if (account is InvestmentAccount investmentAccount)
                {
                    acc.Balance = investmentAccount.AvailableAmount;
                }
                else
                {
                    acc.Balance = account.Balance;
                }
                
                Display.Accounts.Add(acc);
             }
         
            return View(Display);
        }


        [HttpPost]
        // rätt konton ska displayas 
        // är ett konto investment så ska det ej gå och de ska behöva sälja av först 
        public async Task<IActionResult> Transaction(DisplayAllAccountsViewModel model, decimal amount, int to , int from)
        {
            bool check = false;
          
            // visa att det blev fel
            if (from == null || to == null)
            {
                check = true;
                HttpContext.Session.SetString("ErrorMessage", "You need to choose an account, ");
              //  return RedirectToAction("Index");
            }

            if (to == from)
            {
                check = true;
                if (!HttpContext.Session.TryGetValue("ErrorMessage", out byte[] errorMessageBytes))
                {
                    // den är null då måste vi instansiera den
                    HttpContext.Session.SetString("ErrorMessage", "To and from cant be the same, ");
                }
                else
                {
                    HttpContext.Session.SetString("ErrorMessage", HttpContext.Session.GetString("ErrorMessage")
                        + "To and from can't be the same., ");
                }

              
            }
            
           if(amount <= 0) 
            {
                check = true;
                if (!HttpContext.Session.TryGetValue("ErrorMessage", out byte[] errorMessageBytes))
                {
                    // den är null då måste vi instansiera den
                    HttpContext.Session.SetString("ErrorMessage", "You cant transfer a negative number, ");
                }
                else
                {
                    HttpContext.Session.SetString("ErrorMessage", HttpContext.Session.GetString("ErrorMessage")
                       + "You cant transfer a negative number, ");
                }
             
            }
            // ta fram froms balance
            // kolla varför den tar emot fel
            decimal balance =  await _context.bankAccounts.Where(a => a.Id == from)
                          .Select(a => a.Balance)
                          .FirstOrDefaultAsync();
            if (amount > balance)
            {
                check = true;
                if (!HttpContext.Session.TryGetValue("ErrorMessage", out byte[] errorMessageBytes))
                {
                    // den är null då måste vi instansiera den
                    HttpContext.Session.SetString("ErrorMessage", "The amount cant be bigger than the balance of the account, ");
                }
                else
                {
                    HttpContext.Session.SetString("ErrorMessage", HttpContext.Session.GetString("ErrorMessage")
                      + "The amount must be greater than 0, ");
                }

            }

            BankAccount a = await _context.bankAccounts.FirstOrDefaultAsync(b => b.Id == from);
            if (a.AccountType == "Investment")
            {
                check = true;
                if (!HttpContext.Session.TryGetValue("ErrorMessage", out byte[] errorMessageBytes))
                {
                    // den är null då måste vi instansiera den
                    HttpContext.Session.SetString("ErrorMessage", "You need to sell of your investments first, ");
                }
                else
                {
                    HttpContext.Session.SetString("ErrorMessage", HttpContext.Session.GetString("ErrorMessage")
                     + "You need to sell of your investments first, ");
                }
            }
            if(check == true)
            {
                return RedirectToAction("index");
            }

            if(check == false)
            {
                // kolla om användaren är samma då är det en intern transaktion annars extern
                HttpContext.Session.SetString("succesMessage", "Transaction successfull");
                BankAccount fromAccount = new BankAccount();
                BankAccount toAccount = new BankAccount();
                // de involverade kontona
                toAccount = await _context.bankAccounts.FirstOrDefaultAsync(x => x.Id == to);
                fromAccount = await _context.bankAccounts.FirstOrDefaultAsync(x => x.Id == from);
               
                // nytt saldo
                fromAccount.Balance = fromAccount.Balance - amount;
                toAccount.Balance = toAccount.Balance + amount;

                Tran newTran = new Tran();
                newTran.Amount = amount;
                newTran.Date = DateTime.Now;
                newTran.FromBankAccountId = from;
                newTran.ToBankAccountId = to;
                newTran.FromBankAccount = fromAccount;
                newTran.ToBankAccount = toAccount;
                if (toAccount.CustomerId == fromAccount.CustomerId)
                {
                    newTran.Category = "Internal transaction";
                }
                else
                {
                    newTran.Category = "External transaction";
                }
                await _context.transactions.AddAsync(newTran);
                //fixa transactionen till databasen
                await _context.SaveChangesAsync();
               
               

            }

            return RedirectToAction("Index");
        }


        [HttpGet]
       public async Task<IActionResult> AccountsTransactions(int id, string sort , string? date)
        {
            AllAccountsTransactions allTransactions = new AllAccountsTransactions();
            allTransactions.accountsTransactions = new List<AccountsTransactions>();
            DateTime newDate = DateTime.Now;
        

            bool c = false;
            if(date != null)
            {
                 // det är ett datum och då konverterar vi den till ett datum
                if (Validation.IsItADate(date))
                {
                    c = true;
                     newDate = DateTime.Parse(date);
                }
             
            }
          
            var fromTrans = await _context.transactions.Where(c => c.FromBankAccountId == id).ToListAsync();
           
            // ska vara alla from transaktioner
            if (fromTrans.Count() > 0)
            {
                foreach(var tran in fromTrans)
                {
                    AccountsTransactions t = new AccountsTransactions();

                    t.Amount = tran.Amount * -1;
                    t.Date = tran.Date;
                    t.FromBankAccount = await _context.bankAccounts.Where(c => c.Id == tran.FromBankAccountId).Select(c => c.AccountNumber).
                     FirstOrDefaultAsync();
                    t.Category = tran.Category;
                    t.ToBankAccount = await _context.bankAccounts.Where(c => c.Id == tran.ToBankAccountId).
                      Select(c => c.AccountNumber).
                       FirstOrDefaultAsync();

                    t.toAccount = false;

                    allTransactions.accountsTransactions.Add(t);
                }
            }

            var toTrans = await _context.transactions.Where(c => c.ToBankAccountId == id).ToListAsync();

            // ska vara alla to transaktioner
            if (toTrans.Count() > 0)
            {
               
                foreach (var tran in toTrans)
                {
                    AccountsTransactions t = new AccountsTransactions();
                    t.Amount = tran.Amount;
                    t.Date = tran.Date;
                    t.Category = tran.Category;
                    

                    t.FromBankAccount = await _context.bankAccounts.Where(c => c.Id == tran.FromBankAccountId).Select(c => c.AccountNumber).
                        FirstOrDefaultAsync();

                    t.ToBankAccount = await _context.bankAccounts.Where(c => c.Id == tran.ToBankAccountId).
                        Select(c => c.AccountNumber).
                         FirstOrDefaultAsync();

                    t.toAccount = true;

                    allTransactions.accountsTransactions.Add(t);
                }
            }
            var listToSort = new SortListHelperMethods<List<AccountsTransactions>>();

            // sorterar listan
            switch (sort)
            {
                case "date_asc":
                    allTransactions.accountsTransactions = listToSort.SortListByTimeAscending<AccountsTransactions>
                      ((List<AccountsTransactions>)allTransactions.accountsTransactions);
                    break;

                case "date_desc":
                    allTransactions.accountsTransactions = listToSort.SortListByTimeDescending<AccountsTransactions>
                    ((List<AccountsTransactions>)allTransactions.accountsTransactions);
                    break;

                case "amount_asc":
                    allTransactions.accountsTransactions = listToSort.SortListByAmountAscending<AccountsTransactions>
                     ((List<AccountsTransactions>)allTransactions.accountsTransactions);
                    break;

                case "amount_desc":
                    allTransactions.accountsTransactions = listToSort.SortListByAmountDescending<AccountsTransactions>
                      ((List<AccountsTransactions>)allTransactions.accountsTransactions);
                    break;

                case "account_from":
                    string accountNumber = await _context.bankAccounts.Where(c => c.Id == id).Select(c => c.AccountNumber).FirstOrDefaultAsync();
                    // måste skicka med vilket kontonummer man är inne på
                    allTransactions.accountsTransactions = listToSort.SortListByAccountNumberFrom<AccountsTransactions>
                         ((List<AccountsTransactions>)allTransactions.accountsTransactions, accountNumber);
                    break;

                case "account_to":
                    // får fram kontonummer
                    string accountNumb = await _context.bankAccounts.Where(c => c.Id == id).Select(c => c.AccountNumber).FirstOrDefaultAsync();
                    // måste skicka med vilket kontonummer man är inne på
                    allTransactions.accountsTransactions = listToSort.SortListByAccountNumberTo<AccountsTransactions>
                         ((List<AccountsTransactions>)allTransactions.accountsTransactions, accountNumb);
                    break;

                case null:
                    allTransactions.accountsTransactions = listToSort.SortListByTimeDescending<AccountsTransactions>
              ((List<AccountsTransactions>)allTransactions.accountsTransactions);
                    break;
            }
          
            // om datum är korrekt värde
            if (c == true)
                {
                    
                    allTransactions.accountsTransactions = listToSort.SortListByDate<AccountsTransactions>
                       ((List<AccountsTransactions>)allTransactions.accountsTransactions, newDate);

                }


            // när ska denna köras om datum ej är null och c == false
           if(c == false && date != null)
            {
                ViewBag.ErrorMessage = "Date must be of xxxx-xx-xx";
                allTransactions.accountsTransactions = listToSort.SortListByTimeDescending<AccountsTransactions>
                   ((List<AccountsTransactions>)allTransactions.accountsTransactions);
            }

            return View(allTransactions);
        }

        public async Task LoanTransaction(string bankAccount , decimal amount)
        {
            BusinessAccount from = await _context.bankAccounts.OfType<BusinessAccount>().FirstOrDefaultAsync();
            Tran newTran = new Tran();
            newTran.Amount = amount;
            newTran.Date = DateTime.Now;
            // ändra sedan till bankens riktiga konto
            newTran.FromBankAccountId = from.Id;
          BankAccount bankAcc =  await _context.bankAccounts.Where(a => a.AccountNumber.Equals(bankAccount)).FirstOrDefaultAsync();
            newTran.ToBankAccountId =  bankAcc.Id;
            // ändra
            newTran.FromBankAccount = from;
            newTran.ToBankAccount = bankAcc;
            newTran.Category = "Loan";
             await _context.transactions.AddAsync(newTran);

             await _context.SaveChangesAsync();

        }

        public async Task TransferMoney(string accountNumber , decimal amount)
        {
           var account = await _context.bankAccounts.Where(a => a.AccountNumber.Equals(accountNumber)).SingleOrDefaultAsync();
            account.Balance += amount;
            await _context.SaveChangesAsync();
        }

        public async Task PayRent()
        {
            using (var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable))
            {
                try 
                {
                    
                    // hämta alla lån
                    List<Loan> loans = await _context.loans.Where(a => a.LeftToPay > 0).ToListAsync();
                    if (loans.Count == 0)
                    {
                        throw new Exception();
                    }
                    else
                    {
                        foreach (var loan in loans)
                        {
                            // räkna ut räntan
                            decimal rent = loan.LeftToPay * ((loan.InterestRate / 12) * (decimal)0.01);

                            BankAccount account = await _context.bankAccounts.Where(a => a.AccountNumber.Equals(loan.AccountNumber)).FirstOrDefaultAsync();
                            if (account is not null)
                            {
                                (bool isFirstPayment, int days) = await FirstPaymentOrNot(loan);
                                if (isFirstPayment) 
                                {
                                    rent = await NewRent(rent,days);
                                    // nya räntan ska tas fram 
                                    // årsränta / dagar tills betalningen dras
                                }

                                // kolla om det är första betalningen och justera räntan därefter
                                // två metoder en första betalningen
                                // äre det så kallar du på en metod som justerar räntan
                                if (rent < account.Balance)
                                {
                                    await Transaction(account, rent);
                                    await CreateTransaction(account, rent, "Rent");
                                }
                                else
                                {
                                    await _invoiceController.CreateInvoice(rent, loan.CustomerId, loan);
                                    // kalla på metod som skapar en straff-faktura
                                    // användaren kan konvertera den till pdf
                                    // (kod för det har vi redan om den ej raderades)
                                }

                                // en transaction ska skapas kalla på metod
                                // försök dra räntan från kontot
                            }
                            else
                            {
                                throw  new Exception();
                            }


                        }
                    }
                }
                catch (Exception e)
                {

                   await transaction.RollbackAsync();
                }
          
            }
           
          
           

      
      
         
        }

        // justerar de involverades saldo
        public async Task Transaction(BankAccount account , decimal rent)
        {
            try 
            {
               

                IList<Customer> user = await _userManager.GetUsersInRoleAsync("Manager");
                if (user != null)
                {
                    Customer customer = user.FirstOrDefault();
                    var acc = await _context.bankAccounts.OfType<BusinessAccount>().Where(a => a.Customer == customer).FirstOrDefaultAsync();
                    if (acc is null) 
                    {
                        throw new Exception();
                    }
                    account.Balance -= rent;
                    acc.Balance += rent;
                    await _context.SaveChangesAsync();
                }
                else
                {
                    throw new Exception();

                }
            }
            catch(Exception e)
            {
                // vad gör denna 
                throw;
            }
          
           
        }

        // skapar transactionen
        public async Task CreateTransaction(BankAccount account, decimal rent, string category)
        {
           
            try 
            {
               
                var user = await _userManager.GetUsersInRoleAsync("Manager");
                Customer u = user.FirstOrDefault();

                if (user != null)
                {


                  BusinessAccount acc = await _context.bankAccounts.OfType<BusinessAccount>().Where(a => a.Customer == u).FirstOrDefaultAsync();

                    Tran tran = new Tran()
                    {
                        Amount = rent,
                        Date = DateTime.Now,
                        // som inparameter
                        Category = category,
                        FromBankAccountId = account.Id,
                        FromBankAccount = account,
                        ToBankAccountId = acc.Id,
                        ToBankAccount = acc

                    };
                    _context.Attach(tran.FromBankAccount);
                    _context.Attach(tran.ToBankAccount);
                    await _context.transactions.AddAsync(tran);
                    await _context.SaveChangesAsync();







                }
                else
                {
                    throw new Exception();
                }
            }
            catch (Exception e)
            {
                throw;
            }
         
        }

    
        // om de första betalningen o hur många dagar
        private async Task<(bool, int)> FirstPaymentOrNot(Loan loan)
        {
            // om datumet är efter 25 men före månadens slut
            // så 
            bool check = false;
            DateTime currentDate = DateTime.Now;
            DateTime this25 = new DateTime(currentDate.Year, currentDate.Month, 25);
            DateTime targetDate;
            DateTime previousMonth;
            int daysBetween = 0;

            if (currentDate > this25)
            {
                 targetDate = new DateTime(this25.Year, this25.Month, 25);
             
                // är det fortfarande samma månad
                // så kan ej targetdate vara förra månaden utan ska vara denna månadens 25
            }
            else
            {
                previousMonth = currentDate.AddMonths(-1); // Get the previous month
                targetDate = new DateTime(previousMonth.Year, previousMonth.Month, 25);
            }


            if (loan.StartDate > targetDate)
            {
                check = true;
                TimeSpan difference = loan.StartDate - targetDate;
                 daysBetween = (int)difference.TotalDays;
            }

            return (check, daysBetween);
        }

        public async Task<decimal> NewRent(decimal rent,int days)
        {

            int year = DateTime.Now.Year;

            int daysInYear = DateTime.IsLeapYear(year) ? 366 : 365;

            decimal dailyRent = rent / daysInYear;

            decimal newRent = dailyRent * days;


            return newRent;
        }
    }
}
