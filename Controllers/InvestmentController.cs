using Platinum.Areas.Identity.Data;
using Platinum.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NuGet.ContentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using Asset = Platinum.Areas.Identity.Data.Asset;

namespace Platinum.Controllers { 
    [Authorize]
    public class InvestmentController : Controller
    {
        //private readonly AssetApiController _assetApiController;
        private readonly UserManager<Customer> _userManager;
        private readonly MinDbContext _context;

      public InvestmentController(MinDbContext? context, UserManager<Customer>? userManager){
      
       _userManager = userManager;
      _context = context;
        }


        // kommer få göra om denna då vi ska hämta datan ifrån databasen
        [HttpGet]
        public async  Task<IActionResult> Index(string sort, [FromQuery(Name = "Keyword")] string search)
        {
       

            List<Asset> allAssets = await _context.Assets.ToListAsync();
            AllInvestmentsViewModel assets = new AllInvestmentsViewModel();
            assets.Indexes = new List<AssetViewModel>();
            // ska sortera listan
            // föra över assets.indexes till denna lista 

            if (sort != null)
            {
                if (((sort.Equals("3") || sort.Equals("4") || sort.Equals("5")) && search == null))
                {
                    ViewBag.Message = "Category , type and exchange must have a keyword" +
                        " in the search as well";
                }
            }

            foreach (var Asset in allAssets)
            {
                AssetViewModel model = new AssetViewModel();
                model.Price = Asset.Price;
                model.Name = Asset.Name;
                model.Type = Asset.Type;
                model.Risk = Asset.Risk;
                model.Exchange = Asset.Exchange;
                model.Id = Asset.Id;
                model.LastUpdated = Asset.LastUpdated;
                assets.Indexes.Add(model);

            }
            var listToSort = new SortListHelperMethods<List<AssetViewModel>>();
            if (search != null)
            {
                switch (sort)
                {
                    case "1":
                        assets.Indexes = listToSort.SortListByRiskDescending<AssetViewModel>((List<AssetViewModel>)assets.Indexes);
                        break;

                    case "2":
                        assets.Indexes = listToSort.SortListByRiskAscending<AssetViewModel>((List<AssetViewModel>)assets.Indexes);
                        break;

                    case "3":
                        // kategori
                        assets.Indexes = listToSort.SortListByType<AssetViewModel>((List<AssetViewModel>)assets.Indexes, search);
                        break;

                    case "4":
                        // exchange 
                        assets.Indexes = listToSort.SortListByExchange<AssetViewModel>((List<AssetViewModel>)assets.Indexes, search);
                        break;

                    case "5":
                        // namn
                        assets.Indexes = listToSort.SortListByName<AssetViewModel>((List<AssetViewModel>)assets.Indexes, search);
                        break;
                }
            }
            return View(assets);
        }
        
     

        // måste kunna identifiera tillgångslagen på något sätt
        [HttpPost]
        public async Task<IActionResult> BuyOrder(string? acc, decimal balance, int quantity, int id)
        {
        
          
            if (acc == null)
            {
                TempData["ChooseAccount"] = "You need to choose an account";
                return RedirectToAction("Placeorder", new { id = id });
              
            }
           decimal totalAmount =  await _context.Assets.Where(a => a.Id == id).Select(a => a.Price * quantity).FirstOrDefaultAsync();

          InvestmentAccount validAccount =  await _context.bankAccounts.OfType<InvestmentAccount>().Where(a => a.AccountNumber == acc && a.AvailableAmount > totalAmount)
                .FirstOrDefaultAsync();

           if(quantity == 0)
            {
                TempData["BadAmount"] = "Quantity must be an number and bigger than zero";
                return RedirectToAction("Placeorder", new { id = id });
            }
            if(validAccount == null)
            {
                TempData["NotValid"] = "You dont have enough money";
                return RedirectToAction("Placeorder", new { id = id });
            }


          bool check = await  OwningAssetOrNot(id, validAccount.Id);
            CurrentlyOwnedAssets asset = new CurrentlyOwnedAssets();
            if (check)
            {
                 asset = await _context.OwnedAssets.Where(a => a.AssetId == id &&
                a.InvestmentAccountId == validAccount.Id).FirstOrDefaultAsync();
                if (asset != null)
                {
                    asset.Quantity += quantity;
                    validAccount.AvailableAmount -= totalAmount;
                   await _context.SaveChangesAsync();

                }
            
            }
            else
            {
               asset.AssetId = id;
               asset.Quantity = quantity;
               asset.InvestmentAccountId = validAccount.Id;
               validAccount.AvailableAmount -= totalAmount;
                await _context.AddAsync(asset);
               await _context.SaveChangesAsync();

            }

            InvestmentAccountTransactions newTran = new InvestmentAccountTransactions
            {
                AssetId = id,
                InvestmentAccountId = validAccount.Id,
                Quantity = quantity,
                time = DateTime.Now,
                BuyAndSell = true
            };
           await _context.InvestmentsAccountTransactions.AddAsync(newTran);
           await _context.SaveChangesAsync();
           
            TransactionSummaryViewModel model = new TransactionSummaryViewModel();
            model.BuyOrsell = true;
            model.AccountNumber = validAccount.AccountNumber;
            model.Asset = await _context.Assets.Where(a => a.Id == id).Select(a => a.Name).FirstOrDefaultAsync();
            model.Amount = totalAmount;
          

           
            return View("Ordersummary",model);
        }

        private async Task<bool> OwningAssetOrNot(int assetId , int accountId)
        {
            bool check = false;

         var  asset = await _context.OwnedAssets.Where(a => a.InvestmentAccountId == accountId && a.AssetId == assetId)
                .FirstOrDefaultAsync();

            if(asset != null)
            {
                // asset finns
                check =  true;
            }
            return check;
        }

        [HttpPost]
        public async Task<IActionResult> SellOrder(string acc, int quantity, int id)
        {
            if (acc == null)
            {
                TempData["ChooseAccount"] = "You need to choose an account";
                return RedirectToAction("Placeorder", new { id = id });

            }

            if (quantity == 0)
            {
                TempData["BadAmount"] = "Quantity must be an number and bigger than zero";
                return RedirectToAction("Placeorder", new { id = id });
            }

            var account = await _context.bankAccounts.OfType<InvestmentAccount>()
               .Where(a => a.AccountNumber == acc).FirstOrDefaultAsync();

            // den asseten man vill sälja
            var asset = await _context.Assets.Where(a => a.Id == id)
               .FirstOrDefaultAsync();
           
            // byt ut mot metodcall
            bool check = await OwningAssetOrNot(id, account.Id);
           

            if (check == false)
            {
                TempData["DontOwn"] = "You dont own this asset on this account";
                return RedirectToAction("Placeorder", new { id = id });
            }
            var current = await _context.OwnedAssets.Where(a => a.InvestmentAccountId == account.Id
            && a.AssetId == asset.Id).FirstOrDefaultAsync();

            if(current.Quantity < quantity)
            {
                TempData["BigAmount"] = "You dont own that many units of the asset";
                return RedirectToAction("Placeorder", new { id = id });
            }


           current.Quantity -= quantity;
            decimal totalAmount = quantity * asset.Price;
            account.AvailableAmount += totalAmount;
            await _context.SaveChangesAsync();


            InvestmentAccountTransactions newTran = new InvestmentAccountTransactions
            {
                AssetId = id,
                InvestmentAccountId = account.Id,
                Quantity = quantity,
                time = DateTime.Now,
                BuyAndSell = false
            };
            await _context.InvestmentsAccountTransactions.AddAsync(newTran);
            await _context.SaveChangesAsync();

         
            if (current.Quantity == 0)
            {
                _context.OwnedAssets.Remove(current);
                await _context.SaveChangesAsync();
            }

            TransactionSummaryViewModel model = new TransactionSummaryViewModel();
            model.BuyOrsell = false;
            model.AccountNumber = account.AccountNumber;
            model.Asset = await _context.Assets.Where(a => a.Id == id).Select(a => a.Name).FirstOrDefaultAsync();
            model.Amount = totalAmount;



            return View("Ordersummary", model);
        }

   // denna funkar bra
        public async Task SaveAssetToDatabase(Asset a)
        {
            Asset asset = new Asset();
            asset = a;

            // validerar objektet gentemot modelen
            List<ValidationResult> validationResults = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(asset, new ValidationContext(asset), validationResults, true);
            // den finns ej alls något gott snett men api metoden kommer fånga upp det då så denna
            // är redundant
            if (!isValid)
            {
                return;
            }
            
            // om inga tillgångsslag finns så adda det godkända objektet direkt
            // kan kolla namnet redan här
            Asset assets =  await _context.Assets.Where(b => b.Name == a.Name).FirstOrDefaultAsync();

            if (assets == null)
            {
               await _context.Assets.AddAsync(asset);
                await _context.SaveChangesAsync();
                return;

            }
           
            else
            {
                a.Id = assets.Id;
             
               // måste jämföra price och för det är det 2 decimaler

                    // är det en identiskt men namnet är korrekt indikerar på en update
                    if (a != assets)
                    {
                       await UpdateAssetAsync(a ,assets);
                    asset.LastUpdated = a.LastUpdated;
                       await _context.SaveChangesAsync();
                        return;
                    }
                   
            }

        }

        // Denna metod ska funka
        public async Task SaveIndexAssetToDatabase(AllInvestments model)
        {

            // detta ska bort sedan mot den som kommer som input
          AllInvestments m = new AllInvestments();
            m.Indexes = new List<Asset>();
           Asset adda = new Areas.Identity.Data.Asset();
            adda.Price = 10;
            adda.Name = "bombo";
            adda.Type = "baddie";
            adda.Exchange = "lälälä";
            adda.Risk = "sjukt hög";
            m.Indexes.Add(adda);

            if (m.Indexes.Count() == 0 || m == null)
            {
                return;
            }
            foreach(Asset oneIndex in m.Indexes)
            {

                List<ValidationResult> validationResults = new List<ValidationResult>();
                bool isValid = Validator.TryValidateObject(oneIndex, new ValidationContext(oneIndex), validationResults, true);
                if (isValid && oneIndex != null)
                {
                    Asset? assets = await _context.Assets.Where(b => b.Name == oneIndex.Name).FirstOrDefaultAsync();
                    
                 
                    
                    if (assets == null)
                    {
                        await _context.Assets.AddAsync(oneIndex);
                        await _context.SaveChangesAsync();
                        return;
                    }
                    else
                    {
                        oneIndex.Id = assets.Id;

                 
                        if (oneIndex != assets)
                        {
                            await UpdateAssetAsync(oneIndex, assets);

                          
                        }

                    }

                }
            }
        }

        public async Task UpdateAssetAsync(Asset ass ,Asset asset )
        {
            // ass hämtar vi ifrån databasen
            asset.Price = ass.Price;
            asset.Risk = ass.Risk;
            asset.Exchange = ass.Exchange;
            asset.Type= ass.Type;
            asset.LastUpdated = ass.LastUpdated;
            await _context.SaveChangesAsync();
        }

        // bör nog skapa en post också
        [HttpGet]
        public async Task<IActionResult>  PlaceOrder(int id, int acc)
        {
           if (TempData["ChooseAccount"] != null)
            {
                ViewBag.ChooseAccount = TempData["ChooseAccount"];
            }

           if (TempData["NotValid"] != null)
            {
                ViewBag.NotValid = TempData["NotValid"];
            }
            if (TempData["BadAmount"] != null) 
            {
                ViewBag.BadAmount = TempData["BadAmount"];
            }

            if (TempData["DontOwn"] != null)
            {
                ViewBag.DontOwn = TempData["DontOwn"];
            }

            if (TempData["BigAmount"] != null)
            {
                ViewBag.BigAmount = TempData["BigAmount"];
            }

            InvestmentAccount account = await _context.bankAccounts.OfType<InvestmentAccount>().Where(a => a.Id == acc).FirstOrDefaultAsync();
            if(account != null)
            {
                InvestmentAccount a = account;
                ViewBag.AddedAccount = a.AccountNumber;
                ViewBag.Balance = a.AvailableAmount;
            }
       
            var investment = _context.Assets.Find(id);
            AssetViewModel model = new AssetViewModel();
            if (investment != null)
            {
               
                model.Name = investment.Name;
                model.Type = investment.Type;
                model.Risk = investment.Risk;
                model.Exchange = investment.Exchange;
                model.Price = investment.Price;
                model.Id = id;
            }

            return View(model);
        }

        // funkar som den ska!!!
        public async Task<IActionResult> AllOrders(int id)
        {
            AllOrdersViewModel model = new AllOrdersViewModel();
            model.Orders = new List<TransactionSummaryViewModel>();

          var orders = await _context.InvestmentsAccountTransactions
                .Where(a => a.InvestmentAccountId == id).ToListAsync();

        
           string accountNumber = await _context.bankAccounts.OfType<InvestmentAccount>()
                .Where(a => a.Id == id).Select(a => a.AccountNumber).SingleOrDefaultAsync();


            foreach (var order in orders)
            {
                TransactionSummaryViewModel oneorder= new TransactionSummaryViewModel();

                oneorder.AccountNumber = accountNumber;

                oneorder.Asset = await _context.Assets.Where(a => a.Id == order.AssetId).Select(a => a.Name)
                   .SingleOrDefaultAsync(); ;

                oneorder.Amount = order.Quantity;

                oneorder.BuyOrsell = order.BuyAndSell;

                oneorder.time = order.time;

                oneorder.Sum = await _context.Assets.Where(a => a.Id == order.AssetId)
                .Select(a => a.Price * oneorder.Amount).SingleOrDefaultAsync();

                model.Orders.Add(oneorder);
                
            }
            return View(model);
        }

        // gör en metod som visar ett investmentkontos alla innehav och antal
        // denna ska vara klar
        public async Task<IActionResult> AllAssets(int id)
        {
            AllOwnedAssetsViewModel model = new AllOwnedAssetsViewModel();
            
            var AllAssetsOnAccount = await _context.OwnedAssets
                .Where(a => a.InvestmentAccountId == id).ToListAsync();

            string AccountName = await _context.bankAccounts.OfType<InvestmentAccount>().Where(a => a.Id == id)
                .Select(a => a.AccountNumber).SingleOrDefaultAsync();
            decimal totalAmount = 0;
            foreach (var account in AllAssetsOnAccount)
            {
                OwnedAsset asset = new OwnedAsset();
                asset.OnAccount = AccountName;

                 Asset ass =await _context.Assets.Where(a => a.Id == account.AssetId).FirstOrDefaultAsync();

                asset.Name = ass.Name;

                asset.Quantity = account.Quantity;

                asset.Risk = ass.Risk;

                asset.Sum = account.Quantity * ass.Price;

                asset.Type = ass.Type;

                totalAmount += asset.Sum;

                model.OwnedAssets.Add(asset);
            }

            ViewBag.TotalAmount = totalAmount;

            return View(model);
        }
    }
}
