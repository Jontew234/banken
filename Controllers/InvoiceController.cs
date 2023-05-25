using Platinum.Areas.Identity.Data;
using Platinum.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared.Project;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace Platinum.Controllers
{
    [Authorize]
    public class InvoiceController : Controller
    {
        private readonly MinDbContext _context;
        private readonly UserManager<Customer> _userManager;
        private readonly IServiceProvider _serviceProvider;

        public InvoiceController(MinDbContext context, UserManager<Customer> userManager, IServiceProvider serviceProvider)
        {
            _context = context;
            _userManager = userManager;
            _serviceProvider = serviceProvider;
        }
        public async Task<IActionResult> Index()
        {
            if (TempData["AlreadyPayed"] is not null)
            {
                ViewBag.AlreadyPayed = TempData["AlreadyPayed"];
            }
            if (TempData["LowAmount"] is not null)
            {
                ViewBag.LowAmount = TempData["LowAmount"];
            }

            string user = _userManager.GetUserId(User);

            List<Invoice> invoices = await _context.Invoices.Where(a => a.CustomerId == user).ToListAsync();
            ListOfInvoiceViewModel allInvoices = new ListOfInvoiceViewModel();
            allInvoices.invoices = new List<InvoiceViewModel>();
            
            if (invoices.Count == 0)
            {
                ViewBag.NoInvoices = "Theres no available invoices for you";
            }
            else
            {
                List<string> AccountNumbers = await _context.bankAccounts
                   .Where(a => !(a is InvestmentAccount || a is BusinessAccount)).Select(a => a.AccountNumber)
                   .ToListAsync();

                foreach (var invoice in invoices)
                {
                    InvoiceViewModel mod = new InvoiceViewModel();

                    mod.LoansId = invoice.LoansId;
                    mod.Amount = invoice.Amount;
                    mod.LastDayToPay = invoice.LastDayToPay;
                    mod.Payed = invoice.Payed;
                    mod.InvoiceId = invoice.ID;
                    mod.AccountNumbers = AccountNumbers;
                    allInvoices.invoices.Add(mod);
                }
            }
            return View(allInvoices);
        }

        // baka in i transaktion
        public async Task CreateInvoice(decimal amount, string customer, Loan loan)
        {
            // räkna ut summan efter straffavgiften
            decimal newAmount = amount * (decimal)1.1;
            Customer cus = await _context.customers.Where(a => a.Id == customer).FirstOrDefaultAsync();
            if (cus is not null)
            {
                Invoice invoice = new Invoice()
                {
                    Amount = newAmount,
                    Payed = false,
                    LastDayToPay = DateTime.Today.AddDays(30),
                    Customer = cus,
                    CustomerId = customer,
                    Loan = loan,
                    LoansId = loan.ID
                };

                await _context.AddAsync(invoice);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new Exception();
            }


        }
        [HttpGet]
        public async Task<IActionResult> MakePayment(int id, string selectedAccountId, int InvoiceId)
        {
            // om du ej har råd ska du redirecta till index med en tempdata som sedan får displayas där
            decimal balance = await _context.bankAccounts.Where(a => a.AccountNumber.Equals(selectedAccountId)).Select(a => a.Balance).FirstOrDefaultAsync();
            Invoice invoice = await _context.Invoices.FindAsync(InvoiceId);
            if (invoice.Payed == true)
            {
                TempData["AlreadyPayed"] = "This invoice is already payed";
                return RedirectToAction("Index");
            }
            if (balance < invoice.Amount)
            {
                // se till att hantera denna i index och vyn
                TempData["LowAmount"] = "You dont have enough money on this account";
                return RedirectToAction("Index");
            }
            else
            {
                SummaryBeforeInvoicePayment summary = new SummaryBeforeInvoicePayment();
                summary.Amount = invoice.Amount;
                summary.AccountNumber = selectedAccountId;
                summary.ID = InvoiceId;
                return View(summary);
                // visa sammanställning
                // konto
                // id på invoice
                // summan som ska dras och den finns via invoice
                // pengarna ska till business managern
            }

          

        }

        [HttpPost]
        public async Task<IActionResult> MakePayment(SummaryBeforeInvoicePayment model)
        {
            if (ModelState.IsValid)
            {
                Invoice invoice = await _context.Invoices.FindAsync(model.ID);
                // sista som ska ändras
               


                // pengarna ska dras från de angivna kontot och in till managerns konto
                BankAccount account = await _context.bankAccounts.Where(a => a.AccountNumber.Equals(model.AccountNumber)).FirstOrDefaultAsync();
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try 
                    {
                      
                         var scope = _serviceProvider.CreateScope();
                        var _transactionController = scope.ServiceProvider.GetRequiredService
                            <TransactionController>();
                        _transactionController.CreateTransaction(account, model.Amount, "Invoice payment"); 
                       
                        invoice.Payed = true;
                        await _context.SaveChangesAsync();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return BadRequest();
                    }
                 
                }
               
             
            }
            return RedirectToAction("PaymentSucceded");
        }

        public async Task<IActionResult> PaymentSucceded()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ToPdf(string invoice)
        {
            var deserializedModel = JsonConvert.DeserializeObject<InvoiceViewModel>(invoice);


            // kod för att generera pdf

            MemoryStream path = new MemoryStream();

            using (PdfWriter pdfWriter = new PdfWriter(path))
            using (PdfDocument pdfDocument = new PdfDocument(pdfWriter))
            using (Document document = new Document(pdfDocument))
            {
                // Add the invoice details to the document
                document.Add(new Paragraph($"Invoice ID: {deserializedModel.InvoiceId}"));
                document.Add(new Paragraph($"Amount: {deserializedModel.Amount}"));
                document.Add(new Paragraph($"Payed: {(deserializedModel.Payed ? "Yes" : "No")}"));
                document.Add(new Paragraph($"The invoice is for: {deserializedModel.LoansId}"));
                document.Add(new Paragraph($"Last day to pay: {deserializedModel.LastDayToPay}"));

                document.Close();
            }
            var pdfBytes = path.ToArray();
            return File(pdfBytes, "application/pdf", "example.pdf");


            
        }
    }
}
