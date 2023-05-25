using Platinum.Areas.Identity.Data;
using Platinum.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Platinum.Areas.Identity.Data;
using Platinum.Models;
using System.Diagnostics;

namespace Platinum.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly MinDbContext _context;
        private readonly UserManager<Customer> _userManager;

        public HomeController(ILogger<HomeController> logger, MinDbContext context, UserManager<Customer> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {

            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                int unreadMessagesCount = await _context.messages
               .Where(a => a.IsRead == false && a.ReceiverId.Equals(user.Id)).CountAsync();

                HttpContext.Session.SetInt32("UnreadMessagesCount", unreadMessagesCount);


            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}