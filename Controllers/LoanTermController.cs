using Platinum.Areas.Identity.Data;
using Platinum.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Platinum.Controllers
{
    public class LoanTermController : Controller
    {
        MinDbContext _context;
        public LoanTermController(MinDbContext context)
        { 
        _context= context;
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> AddLoanTerms()
        {
            if (TempData["Succed"] is not null)
            {
                ViewBag.Succed = TempData["Succed"];
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddLoanTerms(LoanTermsViewModel model)
        
        {
            List<string> strings= new List<string>();
            
            var terms = await _context.LoanTerms.ToListAsync();
            foreach (var ter in terms)
            {
                if (ter.Name.ToLower().Equals(model.Name.ToLower()))
                {
                    ViewBag.Name = "Name isnt available";
                    return View(model);
                }
            }
           
            if (model.Terms <= 0 || model.Terms > 1 || !decimal.TryParse(model.Terms.ToString(), out _))
            {
                strings.Add("Terms value must be between 0 and 1");
            }
            if (model.Interestrate <= 0 || !decimal.TryParse(model.Terms.ToString(), out _))
            {
                    strings.Add("Interestrate must be a value equals or bigger than zero");  
            }
            ViewBag.TermsError = strings.AsEnumerable();
            if(ViewBag.TermsError.Count > 0 || ViewBag.Name is not null) 
            {
                return View(model);
            }
            if (ModelState.IsValid)
            {
                LoanTerms loanterm = new LoanTerms()
                {
                    Name= model.Name,
                    Interestrate= model.Interestrate,
                    IsActive= model.IsActive,
                    Terms= model.Terms,
                };
                _context.LoanTerms.AddAsync(loanterm);
                TempData["Succed"] = "New Loanterm added";
                await _context.SaveChangesAsync();
                return RedirectToAction("AddLoanTerms");

            }
            else
            {
                return View(model);
            }
           
          
        }


        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Index()
        {
            // hämta alla för över till viewmodel
            List<LoanTerms> terms = await _context.LoanTerms.ToListAsync();
            ListOfLoanTermsViewModel list = new ListOfLoanTermsViewModel();
            list.LoanTerms = new List<LoanTermsViewModel>();
            foreach (var term in terms)
            {
                LoanTermsViewModel mod = new LoanTermsViewModel()
                {
                    Id = term.Id,
                    Terms = term.Terms,
                    Name= term.Name,
                    Interestrate=term.Interestrate,
                    IsActive=term.IsActive
                };
                list.LoanTerms.Add(mod);
            }
            return View(list);
        }
    }
}
