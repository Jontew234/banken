using Platinum.Areas.Identity.Data;
using Platinum.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

using System.Data;
using System.Text.RegularExpressions;

namespace Platinum.Controllers
{
    [Authorize]
    public class CustomerController : Controller
    {
        private UserManager<Customer> _userManager;
        private readonly MinDbContext _context;
        private RoleManager<IdentityRole> _roleManager;
        private SignInManager<Customer> _signInManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationPolicyProvider _policyProvider;


        public CustomerController(MinDbContext context, UserManager<Customer> userManager, RoleManager<IdentityRole> roleManager, SignInManager<Customer> signInManager, IHttpContextAccessor httpContextAccessor, IAuthorizationPolicyProvider policyProvider)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _context = context;
            _signInManager = signInManager;
            _httpContextAccessor = httpContextAccessor;
          
            _policyProvider = policyProvider;
        }


        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> CreateRole()
        {
           if(TempData["Sucess"] is not null)
            {
                ViewBag.Sucess = TempData["Sucess"];
            }
            return View();
        }

       [Authorize(Roles = "Admin")]
       [HttpPost]
        public async Task<IActionResult> CreateRole(CreateRoleViewModel model)
        {
            bool roleExists = await _roleManager.RoleExistsAsync(model.RoleName);

            if (ModelState.IsValid && !roleExists)
            {
                IdentityRole identityRole = new IdentityRole
                {
                    Name = model.RoleName

                };

          IdentityResult result =  await _roleManager.CreateAsync(identityRole);
                if (result.Succeeded)
                {
                    TempData["Sucess"] = "Role created succesfully";
                    return RedirectToAction("CreateRole","Customer","GET");
                }
            }
            else
            {
                ViewBag.RoleName = "The role already exists";
            }
             
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminStartPage()
        {
            return View();
        }

       
      public async Task<IActionResult> ChooseRole(string id)
        {
            var roles = await _roleManager.Roles.ToListAsync();
            AllRoles allRoles = new AllRoles();
            foreach (var role in roles)
            {
                OneRole r = new OneRole();
                r.Id = role.Id;
                r.Name = role.Name;
                allRoles.Roles.Add(r);

            }
            ViewBag.Id = id;
            return View(allRoles);
           
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> AddUserToRole(string userId,string roleName)
        {
            
            var user  = await _userManager.FindByNameAsync(userId);

            var role = await _roleManager.FindByNameAsync(roleName);
            if(role != null || user != null)
            {
              
                if (roleName.Equals("Manager"))
                {
                    var usersInRole = await _userManager.GetUsersInRoleAsync(roleName);
                    if (usersInRole.Count == 0)
                    {
                        await _userManager.AddToRoleAsync(user, role.Name);
                        return RedirectToAction("AllUsers");
                    }
                    else
                    {
                        TempData["AlreadyTaken"] = "Only one user can have this role";
                        return RedirectToAction("AllUsers");
                    }



                }
               
              
                await _userManager.AddToRoleAsync(user, role.Name);

                return RedirectToAction("AllUsers");

               
            }
            else
            {
                TempData["Error"] = "Unexpected Error";
                return RedirectToAction("AllUsers");

            }
    }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AllRoles()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            AllRoles allRoles = new AllRoles();
            foreach(var role in roles)
            {
                OneRole r = new OneRole();
                r.Id = role.Id;
                r.Name = role.Name; 
                allRoles.Roles.Add(r);

            }
                
            return View(allRoles);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AllUsers(string sort , string search)
        {
            if (TempData["Error"] != null)
            {
                ViewBag.Error = TempData["Error"];
            }
            if (TempData["AlreadyTaken"] != null)
            {
                ViewBag.AlreadyTaken = TempData["AlreadyTaken"];
            }
            var users = await _userManager.Users.ToListAsync();
            if (search == null )
            {
                ViewBag.Search = "the searchstring is empty";
            }
         
            if (search != null && sort.Equals("2"))
            {
                users = await _userManager.Users.Where(a => a.UserName.Equals(search)).ToListAsync();
                if (users.Count == 0)
                {
                    ViewBag.Name = "Theres no user with this username";
                }
            }
            if (search != null && sort.Equals("1"))
            {
                users = _userManager.GetUsersInRoleAsync(search).Result.ToList();
                if (users.Count == 0)
                {
                    ViewBag.Role = "No one has this role";
                }
            }
            // Retrieve all users

            AllUsers allUsers = new AllUsers();
          
            foreach (var user in users)
            {
                OneUser oneUser = new OneUser();
                var roles = await _userManager.GetRolesAsync(user);
                string roleNames = string.Join(", ", roles);

                var lockoutEndDate = await _userManager.GetLockoutEndDateAsync(user);
                oneUser.IsLocked = lockoutEndDate != null && lockoutEndDate > DateTimeOffset.UtcNow;

                oneUser.Roles = roleNames;
                oneUser.UserName = user.UserName;
                allUsers.Users.Add(oneUser);
           
            }
          

            return View(allUsers);

        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (TempData["First"] is not null)
            {
                ViewBag.First = TempData["first"];
            }
            if (TempData["Last"] is not null)
            {
                ViewBag.Last = TempData["Last"];
            }
            ChangeDataViewModel model = new ChangeDataViewModel();
            string i = _userManager.GetUserId(User);
           

         
          model.FirstName = await _context.Users.Where(u => u.Id == i).Select(u => u.FirstName).FirstOrDefaultAsync();
           model.LastName = await _context.Users.Where(u => u.Id == i).Select(u => u.LastName).FirstOrDefaultAsync();
            model.PhoneNumber =  await _context.Users.Where(u => u.Id == i).Select(u => u.PhoneNumber).FirstOrDefaultAsync();
            model.Address = await _context.Users.Where(u => u.Id == i).Select(u => u.Address).FirstOrDefaultAsync();

   
           return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveChanges(ChangeDataViewModel model)
        {
            bool check = false;
            bool LettersFirst = Regex.IsMatch(model.FirstName, @"^[a-zA-Z]+$");
            bool LettersLast = Regex.IsMatch(model.LastName, @"^[a-zA-Z]+$");
            if(LettersFirst == false)
            {
                TempData["First"] = "Firstname must be letters only";
                check = true;
            }
            if (LettersFirst == false)
            {
                TempData["Last"] = "Lastname must be letters only";
                check = true;
            }
            if (check == true) 
            {
                return RedirectToAction("Index");
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                

                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.PhoneNumber = model.PhoneNumber;
                user.Address = model.Address;

                await _userManager.UpdateAsync(user);
                await _context.SaveChangesAsync();
                TempData["message"] = "Data updated successfully.";
                return RedirectToAction("Index");
            }

            return View("Index");
        }

        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if(ModelState.IsValid)
            {
                var result = await ChangePassAsync(model);
                if (result.Succeeded)
                {
                    ViewBag.IsSuccess = true;
                    ModelState.Clear();
                    return View();
                }

            }

            return View(model);

        }



        public async Task<IdentityResult> ChangePassAsync(ChangePasswordViewModel model)
        {
           string i =  _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(i);

           return await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

           
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> InActivate(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            DateTimeOffset lockoutTime = DateTime.MaxValue;
            var result = await _userManager.SetLockoutEndDateAsync(user,lockoutTime);

            return RedirectToAction("AllUsers");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Activate(string userName)
        {

            var user = await _userManager.FindByNameAsync(userName);
            DateTimeOffset lockoutTime = new DateTimeOffset(new DateTime(2022, 1, 1), TimeSpan.Zero);
            ;
            var result = await _userManager.SetLockoutEndDateAsync(user, lockoutTime);
            return RedirectToAction("AllUsers");
        }

      public async  Task<IActionResult> DeleteUserFromRole(string userId, string roleName)
        {
            var user = await _userManager.FindByNameAsync(userId);

            var role = await _roleManager.FindByNameAsync(roleName);

            var result = await _userManager.RemoveFromRoleAsync(user, role.Name);

            return RedirectToAction("AllUsers");
        }

        //[HttpPost]
        //public async Task<IActionResult> LogOutUser()
        //{
           

        //    try
        //    {
        //        HttpContext http = _cache.Get<HttpContext>("Current");
        //        if (http != null)
        //        {
        //            await http.SignOutAsync();
        //            http.Session.Clear();
        //            return RedirectToAction("index", "Home");
        //        }


        //    }
            
            
        //  catch (Exception e)
        //    {

        //    }

        //    return RedirectToAction("index","Home");
        //}
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> ManagerStartPage()
        {
            return View();
        }

       
    }
}
