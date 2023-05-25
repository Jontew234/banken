using Platinum.Areas.Identity.Data;
using Platinum.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace Platinum.Controllers
{
    [Authorize]
    public class CardController : Controller
    {
        private readonly UserManager<Customer> _userManager;
        private readonly MinDbContext _context;

        public CardController(MinDbContext context, UserManager<Customer> userManager)
        {

            _userManager = userManager;
            _context = context;
        }


        [HttpGet]
        public async Task<IActionResult> Index()
        {

           
            
           CardViewModel m = new CardViewModel();
            m.Cards = new List<DisplayCardViewModel>();
            string user = _userManager.GetUserId(User);
            IList<Card> cards =  await _context.cards.Where(c => c.CustomerId == user).ToListAsync();
            
            
                foreach (var c in cards)
                {
                    DisplayCardViewModel model = new DisplayCardViewModel();
                    model.CardNumber = c.CardNumber;
                    model.ExpirationMonth = c.ExpirationMonth;
                    model.ExpirationYear = c.ExpirationYear;
                    model.CVV = c.CVV;
                    model.OnlinePurchase = c.OnlinePurchase;
                    model.Active = c.Active;
                    model.Id = c.Id;
                    m.Cards.Add(model);
                }
            
            return  View(m);
        }

        [HttpGet]
        public IActionResult Add() {

            return View();
        }

        // vad returnerar vi om något blir fel?
        [HttpPost]
        public async Task<IActionResult> Add(AddCardViewModel model)
        {
            bool check = false;
            List<Card> cards = _context.cards.ToList();
            foreach (var card in cards)
            {
                
                if (card.CardNumber == model.CardNumber)
                {
                    check = true;
                    ViewBag.ErrorMessage = "Cardnummer is taken,";
                   
                }
                if (card.CVV == model.CVV)
                {
                    check = true;
                    if (ViewBag.ErrorMessage == null)
                    {
                        ViewBag.ErrorMessage = "CVC is already taken";
                    }
                    else
                    {
                        ViewBag.ErrorMessage += "CVC is already taken";
                    }
                }

            }
            if (check == true)
            {
                return View();
            }
            
            if (ModelState.IsValid)
            {
                Card card = new Card();

                card.CVV = model.CVV;
                card.CardNumber = model.CardNumber;
                card.ExpirationMonth = model.ExpirationMonth;
                card.ExpirationYear = model.ExpirationYear;
                card.OnlinePurchase = true;
                card.Active = true;
                string user = _userManager.GetUserId(User);
                card.Customer =  await _context.customers.Where(c => c.Id == user).FirstOrDefaultAsync();
                card.CustomerId = user;
                _context.cards.AddAsync(card);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            return View();
        }



        // denna skickar med rätt id
        [HttpGet]
      
        public async Task<IActionResult> EditCard(int id)

        {
            
            var card = await _context.cards.FindAsync(id);
           if (card == null)
            {
                return NotFound();
            }

           else
            {
                // för över till modellen som ska displayas
                EditCardViewModel model = new EditCardViewModel();
                model.Active = card.Active;
                model.OnlinePurchase = card.OnlinePurchase;
                model.Id = card.Id;
                model.CardNumber = card.CardNumber;

           
                return View(model);
            }
        }
        [HttpPut]
        [HttpPost]
        [ValidateAntiForgeryToken]

        // de ändrade värdena kommer in
        public async Task<IActionResult> EditCard(EditCardViewModel model)
        {
            // id för att söka upp kortet och ändra bör vara fixat
            var card = await _context.cards.FindAsync(model.Id);
            if (card == null)
            {
                return NotFound();
            }
            else {
                card.OnlinePurchase = model.OnlinePurchase;
               
                card.Active = model.Active;
                card.Active = model.Active;
                await _context.SaveChangesAsync();
                ViewBag.Success = "Card edited succesfully";

            }
            return View(model);
        }




        // om inget kort är kopplat så kan vi dirigera direkt till radera (köra den actionmetoden)
        [HttpGet]
        public async Task<IActionResult> IsCardLinkedToAccount(int cardId)
        {
           //  alla konton som är kopplade till ett specifikt kort det kortet vi ska radera
            // är denna 0 eller null kan vi skicka den direkt
           IList<CardAccount> cards = await _context.CardAccounts.Where(c => c.CardId == cardId).ToListAsync();
            if(cards.Count() == 0)
            {
                // måste få med parametrar
                // måste få med en editviewmodel här
                // rätt id här
                return RedirectToAction("Delete", "Card", new { id = cardId });


            }

            // hämta alla konton 

            IList<BankAccount> accounts =  await _context.bankAccounts.ToListAsync();


            List<BankAccount> LinkedAccounts = new List<BankAccount>();
          //  loopar alla kort och tar ut alla kopplade bankonton till kortet vi vill radera
            foreach (var c in accounts)
            {
               foreach (var ca in cards)
                {
                   if (c.Id == ca.BankAccountId)
                    {
                        if (c != null)
                        {
                            LinkedAccounts.Add(c);
                        }
                       

                    }
                }

           }
            // behöver en model som kan displayas
            ListofCardsLinkedToAccount allAccounts= new ListofCardsLinkedToAccount();
           allAccounts.accounts = new List<CardsLinkedToAccount>();
            foreach (var c in LinkedAccounts)
            {
                CardsLinkedToAccount account = new CardsLinkedToAccount();
                account.Id = c.Id; 
                account.AccountNumber = c.AccountNumber;
                account.AccountType = c.AccountType;
               account.CardId = cardId;
               allAccounts.accounts.Add(account);
                
            }
         
            
                return View(allAccounts);
            
        }


        // kolla så kortet ej är kopplat till ett konto
        // ska ändå gå att ta bort men varningsskylt ska displayas


        public  async Task<IActionResult> Delete(int id)
        {
            Card card =  await _context.cards.FirstOrDefaultAsync(c => c.Id == id);


            if (card != null)
            {
               _context.cards.Remove(card);
             await _context.SaveChangesAsync();
                
            }

            return RedirectToAction("Index", "Card");
        }

        // denna ska köras när man raderar ett kort direkt
        public async Task<IActionResult> RemoveCardFromAccount(int cardId)
        {
            // kortet kan vara kopplat till flera konton
            // behöver bara kortid på det man vill radera 
            // kan vara flera
        List<CardAccount> obj = await _context.CardAccounts.Where(c => c.CardId == cardId).ToListAsync();
           
            foreach(var o in obj)
            {
               _context.CardAccounts.Remove(o);
                await _context.SaveChangesAsync();
            }
    
          
            return RedirectToAction("Delete", "Card", new { id = cardId });

        }



    }
}
