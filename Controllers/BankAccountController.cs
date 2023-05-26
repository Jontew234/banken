using Platinum.Areas.Identity.Data;
using Platinum.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.EntityFrameworkCore;
using System.Security.Principal;
using System.IO;
using System.ComponentModel.DataAnnotations;
using iText.Forms.Xfdf;


namespace Platinum.Controllers
{
    [Authorize]
    public class BankAccountController : Controller
    {
        private readonly UserManager<Customer> _userManager;
        private readonly MinDbContext _context;
        private readonly AssetApiController api;

        public BankAccountController(MinDbContext? context, UserManager<Customer>? userManager, AssetApiController assetApiController)
        {

            _userManager = userManager;
            _context = context;
            api = assetApiController;
        }

        public async Task<IActionResult> AddBusinessAccount()
        {
            return View();

        }

        [HttpPost]
        public async Task<IActionResult> AddBusinessAccount(AddBankAccountViewModel model)
        {

            List<BankAccount> allAccounts = await _context.bankAccounts.ToListAsync();
            foreach (BankAccount bankAccount in allAccounts)
            {

                if (bankAccount.AccountNumber == model.AccountNumber)
                {
                    ViewBag.Error = "Accountnummer is already available";
                    return View();
                }
            }
            int counter = _context.bankAccounts.OfType<BusinessAccount>().Where(a => a.IsActive == true).Count(); 
            if (counter > 0)
            {
               ViewBag.Taken = "There can just be one businessAccount";
                return View();
            }
           

            foreach (var modelStateEntry in ModelState)
            {
                if (modelStateEntry.Value.Errors.Any())
                {
                    foreach (var error in modelStateEntry.Value.Errors)
                    {
                        Console.WriteLine($"Error for property {modelStateEntry.Key}: {error.ErrorMessage}");
                    }
                }
            }

          
            if (ModelState.IsValid)
            {

                string user = _userManager.GetUserId(User);
                Customer customer = await _context.customers.SingleOrDefaultAsync(c => c.Id == user);
                BusinessAccount account = new BusinessAccount();
               
               
                 
                    account.CustomerId = user;
                    account.Customer = customer;
                    account.AccountNumber = model.AccountNumber;
                    account.AccountType = model.AccountType;
                    account.Balance = model.Balance;
                    account.IsActive = true;


                    await _context.bankAccounts.AddAsync(account);
                   int result = await _context.SaveChangesAsync();
                if (result > 0)
                {
                    ViewBag.Success = "Account created";
                }
                return View();
            }


            return View();

        }
        [HttpGet]
        public async Task<IActionResult> Index(int sort)
        {
           
         


            if (TempData["Transfer"] != null)
                {

                    ViewBag.Transfer = TempData["Transfer"];
                }

       

            if (TempData["Cards"] != null)
            {

                ViewBag.Cards = TempData["Cards"];
            }

            if (TempData["SellOf"] != null)
            {

                ViewBag.SellOf = TempData["SellOf"];
            }

            if (TempData["Error"] != null)
            {

                ViewBag.ErrorMessage = TempData["Error"];
            }
            DisplayAccountViewModel mod = new DisplayAccountViewModel();
            mod.Accounts = new List<AddBankAccountViewModel>();

            // oklart varför den ej vill async
            string user = _userManager.GetUserId(User);
            List<BankAccount> bankAccounts = await _context.bankAccounts.Where(c => c.CustomerId == user && c.IsActive == true && !(c is BusinessAccount)).ToListAsync();
            foreach (BankAccount bankAccount in bankAccounts)
            {
                AddBankAccountViewModel model = new AddBankAccountViewModel();
                model.AccountType = bankAccount.AccountType;
                model.AccountNumber = bankAccount.AccountNumber;
                if (bankAccount is InvestmentAccount)
                {
                    InvestmentAccount investmentAccount = (InvestmentAccount)bankAccount;
                    model.Balance = investmentAccount.AvailableAmount;

                }
                else 
                {
                    model.Balance = bankAccount.Balance;
                }
          
                model.Id = bankAccount.Id;
                model.HasCard = await AddedCardsCheck(bankAccount.Id);
                mod.Accounts.Add(model);

            }
            var listToSort = new SortListHelperMethods<List<AddBankAccountViewModel>>();
            if (sort == 1)
            {
                string search = "Savings";
              mod.Accounts = listToSort.SortListByAccounType<AddBankAccountViewModel>(mod.Accounts, search);

            }

            if (sort == 2)
            {
                string search = "Investment";
                mod.Accounts = listToSort.SortListByAccounType<AddBankAccountViewModel>(mod.Accounts, search);

            }

            if (sort == 3)
            {

                string search = "Checking";
                mod.Accounts = listToSort.SortListByAccounType<AddBankAccountViewModel>(mod.Accounts, search);

            }

            return View(mod);
        }

            private async Task<bool> AddedCardsCheck(int accountId)
            {
                // kolla så att linq tar fram rätt data
                // bör vara korrekt
                IList<CardAccount> cards = await _context.CardAccounts.Where(c => c.BankAccountId == accountId).ToListAsync();
                return cards.Count() > 0;
            }

            [HttpGet]
            public IActionResult AddAccount()
            {

                return View();
            }

            [HttpPost]
            public async Task<IActionResult> AddAccount(AddBankAccountViewModel model)
            {
                List<BankAccount> allAccounts = await _context.bankAccounts.ToListAsync();
                foreach (BankAccount bankAccount in allAccounts) {

                    if (bankAccount.AccountNumber == model.AccountNumber)
                    {
                        TempData["Error"] = "Accountnummer is already available";
                        return RedirectToAction("Index");
                    }
                }

                if (ModelState.IsValid)
                {

                    string user = _userManager.GetUserId(User);
                    Customer customer = await _context.customers.SingleOrDefaultAsync(c => c.Id == user);
                    BankAccount account;
                    if (model.AccountType.Equals("Investment"))
                    {
                        InvestmentAccount accountinvest = new InvestmentAccount();
                        accountinvest.CustomerId = user;
                        accountinvest.Customer = customer;
                        accountinvest.AccountNumber = model.AccountNumber;
                        accountinvest.AccountType = model.AccountType;
                        accountinvest.Balance = model.Balance;
                        accountinvest.IsActive = true;
                        accountinvest.AvailableAmount = model.Balance;
                   
                        await _context.bankAccounts.AddAsync(accountinvest);
                        await _context.SaveChangesAsync();
                       
      
                   

                    }
                    else
                    {
                        account = new BankAccount();
                        account.CustomerId = user;
                        account.Customer = customer;
                        account.AccountNumber = model.AccountNumber;
                        account.AccountType = model.AccountType;
                        account.Balance = model.Balance;
                        account.IsActive = true;
                       

                        await _context.bankAccounts.AddAsync(account);
                        await _context.SaveChangesAsync();

                    }


                    return RedirectToAction("Index");
                }


                return View();
            }

            [HttpGet]
            public async Task<IActionResult> AllCards(int id)
            {

                AllViewModel m = new AllViewModel();
                m.Cards = new List<AllCardsViewModel>();
                string user = _userManager.GetUserId(User);

                IList<CardAccount> card = _context.CardAccounts.Where(c => c.BankAccountId == id).ToList();
                // användarens alla kort
                // om ett av användarens kort är kopplat till kontot så kan man ta bort den ifrån listan med  ALLA kort
                IList<Card> cards = await _context.cards.Where(c => c.CustomerId == user).ToListAsync();
                IList<Card> newCard = new List<Card>();
                // hämtar bara ut listan med alla kort som ej matchar bankkontot
                // lägger till alla kort i systemet som ej är kopplade till nuvarande konto men finns i sambandstabellen
                List<Card> cardsToRemove = new List<Card>();
                foreach (var c in card)
                {
                    foreach (var ca in cards)
                    {
                        if (c.CardId == ca.Id)
                        {
                            cardsToRemove.Add(ca);

                        }
                    }

                }
                foreach (var c in cardsToRemove)
                {
                    cards.Remove(c);
                }


                foreach (var c in cards)
                {
                    AllCardsViewModel model = new AllCardsViewModel();
                    model.CardNumber = c.CardNumber;
                    model.Id = c.Id;
                    model.Account = id;
                    m.Cards.Add(model);

                }
                return View(m);
            }
            [HttpGet]
            public async Task<IActionResult> AddCardToAccount(int AccountId, int CardId)
            {
                CardAccount cardAcc = new CardAccount
                {
                    BankAccountId = AccountId,
                    CardId = CardId
                };
                await _context.CardAccounts.AddAsync(cardAcc);
                await _context.SaveChangesAsync();


                return RedirectToAction("index");


            }

            // inpur är ett bankkontoid
            [HttpGet]
            public async Task<IActionResult> CardsLinkedToAccount(int id) {

                // måste få ut alla kort som är kopplat till id alltså ett bankonto

                IList<CardAccount> cards = await _context.CardAccounts.Where(c => c.BankAccountId == id).ToListAsync();

                AllViewModel m = new AllViewModel();
                m.Cards = new List<AllCardsViewModel>();

                // alla kort
                IList<Card> card = await _context.cards.ToListAsync();
                // loppa igenom alla kort i sambandstabellen
                // ska ta ut alla de korten som finns i sambandstabellen 
                // kopplingen till kontot är redan gjort
                List<Card> cardsToDisplay = new List<Card>();
                foreach (var c in card)
                {
                    foreach (var ca in cards)
                    {
                        if (c.Id == ca.CardId)
                        {
                            cardsToDisplay.Add(c);

                        }
                    }

                }
                foreach (var c in cardsToDisplay)
                {
                    AllCardsViewModel model = new AllCardsViewModel();
                    model.CardNumber = c.CardNumber;
                    model.Id = c.Id;
                    model.Account = id;
                    m.Cards.Add(model);

                }
                return View(m);
            }

            [HttpPost]
            [ValidateAntiForgeryToken]
            [HttpDelete]
            public async Task<IActionResult> RemoveCardFromAccount(int accountid, int CardId)
            {


                var cards = await _context.CardAccounts.FirstOrDefaultAsync(c => c.CardId == CardId && c.BankAccountId == accountid);
                if (cards != null)
                {

                    _context.CardAccounts.Remove(cards);
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction("index");
            }


            // bör vara klar men skapa en länk hit och debugga
            public async Task<IActionResult> AllInvestmentsAccount(int id)
            {

                // denna displayar vyn i en get men sedan kan vi ha en post som displayar oss vidare
                // men id som vi får som input måste komma med i vyn
                ViewBag.Id = id;

                string user = _userManager.GetUserId(User);
                List<InvestmentAccount> allInvestmentAccounts = await _context.bankAccounts.OfType<InvestmentAccount>().
                    Where(a => a.CustomerId == user).ToListAsync();

                AllInvestmentAccountViewModel model = new AllInvestmentAccountViewModel();
                model.InvestmentAccounts = new List<AccountViewModel>();

                if (!allInvestmentAccounts.Any()) {
                }
                foreach (var account in allInvestmentAccounts)
                {
                    AccountViewModel oneAccount = new AccountViewModel();
                    oneAccount.Id = account.Id;
                    oneAccount.AccountNumber = account.AccountNumber;
                    oneAccount.AccountType = account.AccountType;
                    oneAccount.Balance = account.AvailableAmount;
                    model.InvestmentAccounts.Add(oneAccount);

                }
                return View(model);



            }

            public async Task<IActionResult> deleteAccount(int id)
            {
                // sök upp kontot
                var account = await _context.bankAccounts.Where(a => a.Id == id).SingleOrDefaultAsync();
                // får den upp båda? det borde den göra!
              
                if (account is InvestmentAccount)
                {
                    InvestmentAccount investmentAccount = (InvestmentAccount)account;
                    bool check = false;

                    if (investmentAccount.AvailableAmount > 0)
                    {
                        check = true;
                        TempData["Transfer"] = "You need to tranfer the money on this account to another account";

                    }
                    // kolla om kontot finns i currently owned assets
                    int ownedasset = await _context.OwnedAssets.Where(a => a.InvestmentAccountId == id).CountAsync();
                    if (ownedasset > 0)
                    {
                        check = true;
                        TempData["SellOf"] = "You need to sell of your assets first";
                    }

                    if (check == true)
                    {
                        // redirecta till alla konton vyn som för över tempdata till 
                        // viewbag som sedan displayas i vyn
                        return RedirectToAction("index");
                    }
                    investmentAccount.IsActive = false;
                    await _context.SaveChangesAsync();
                }



                else
                {
                    bool checking = false;
                    if (account.Balance != 0)
                    {
                        TempData["Transfer"] = "You need to tranfer the money on this account to another account";
                        checking = true;
                    }

                    int check = await _context.CardAccounts.Where(a => a.BankAccountId == id).CountAsync();
                    if (check > 0)
                    {
                        TempData["Cards"] = "You have added cards on this account that you need to remove";
                        checking = true;
                    }
                    if (checking == true)
                    {

                        return RedirectToAction("index");
                    }

                    account.IsActive = false;
                    await _context.SaveChangesAsync();
                }
            if (account is BusinessAccount)
            {
                return RedirectToAction("BusinessAccountInfo");
            }
            else 
            {
                return RedirectToAction("index");
            }
               
            }
            
        public async Task<IActionResult> BusinessAccountInfo()
        {
            DisplayAccountViewModel mod = new DisplayAccountViewModel();
            mod.Accounts = new List<AddBankAccountViewModel>();
            List<BusinessAccount> bankAccounts = await _context.bankAccounts.OfType<BusinessAccount>().Where(a => a.IsActive == true).ToListAsync();
            if (bankAccounts.Count != 0)
            {
                foreach (BusinessAccount bankAccount in bankAccounts)
                {
                    AddBankAccountViewModel model = new AddBankAccountViewModel();
                    model.AccountType = bankAccount.AccountType;
                    model.AccountNumber = bankAccount.AccountNumber;
                    model.Balance = bankAccount.Balance;
                    model.Id = bankAccount.Id;

                    mod.Accounts.Add(model);
                    return View(mod);
                }
            }
           
            return View(mod);
        }

        }

    }
