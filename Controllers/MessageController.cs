using Platinum.Areas.Identity.Data;
using Platinum.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore;
using NuGet.ContentModel;
using NuGet.Protocol.Plugins;

namespace Platinum.Controllers
{
    [Authorize]
    public class MessageController : Controller
    {
        private readonly UserManager<Customer> _userManager;
        private readonly MinDbContext _context;
        public MessageController(UserManager<Customer> userManager, MinDbContext context) 
        
        {
            _userManager = userManager;
            _context = context;
          
    }
        public async Task<IActionResult> Index(string sort, string search)
        {
            int option = 0;
            ListOfMessageViewModel listOfMessage = new ListOfMessageViewModel();
            listOfMessage.ListOfMessages = new List<MessageViewModel>();
         var user = await _userManager.GetUserAsync(User);
         List<Areas.Identity.Data.Message> messages = await _context.messages.Where(a => a.Receiver == user).ToListAsync();
            foreach (var message in messages)
            {
                MessageViewModel mess = new MessageViewModel()
                {
                    Id = message.Id,
                    Subject = message.Subject,
                    Body = message.Body,
                    SentDate = message.SentDate,
                    IsRead = message.IsRead,
                    ReceiverId = message.ReceiverId,
                    Sender = await _context.customers.Where(a => a.Id == message.SenderId).Select(a => a.Email).FirstOrDefaultAsync()

                }; 
                listOfMessage.ListOfMessages.Add(mess);
               
               
            }

            if (sort is not null)
            {
                option = int.Parse(sort);
            }
            
            listOfMessage = await sortlist(option,search ,listOfMessage);

            return View(listOfMessage);
        }

        // här måste vi prefilla värdena
        public async Task<IActionResult> SendMessage(string? subject, string receiver)
        {

            ViewBag.Sucess = TempData["Success"];
            SendMessageViewModel messageViewModel = new SendMessageViewModel();
            messageViewModel.Receivers = await _context.customers.ToListAsync();
            messageViewModel.Subject = subject;
            messageViewModel.PreSelectedId = receiver;
            return View(messageViewModel);
        }

      

        [HttpPost]
        public async Task<IActionResult> SendMessage(SendMessageViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
         
            if (ModelState.IsValid)
            {
                foreach(var u in model.SelectedReceiverIds) 
                {
                    Areas.Identity.Data.Message mess = new Areas.Identity.Data.Message();
                    mess.Subject = model.Subject;
                    mess.Body = model.Body;
                    mess.Sender = user;
                    mess.SenderId = user.Id;
                    mess.SentDate = DateTime.Now;
                    mess.Receiver = await _context.customers.Where(a => a.Id == u).FirstOrDefaultAsync(); ;
                    mess.ReceiverId = u;
                    mess.SenderId = user.Id;
                    mess.Sender = user;
                    mess.IsRead = false;

                    await _context.messages.AddAsync(mess);
                    await _context.SaveChangesAsync();

                   

                    int unreadMessagesCount = await _context.messages
                   .Where(a => a.IsRead == false && a.ReceiverId.Equals(user.Id)).CountAsync();

                    HttpContext.Session.SetInt32("UnreadMessagesCount", unreadMessagesCount);
                }
               
            }
            TempData["Success"] = "Message has been sent";
            return RedirectToAction("SendMessage");
        }

        public async Task<IActionResult> DeleteMessage(int messageId)
        {
            Areas.Identity.Data.Message message = await _context.messages.Where(a => a.Id == messageId).FirstOrDefaultAsync();
            _context.messages.Remove(message);
            await _context.SaveChangesAsync();
            if(message.IsRead == false)
            {
                var user = await _userManager.GetUserAsync(User);
                int unreadMessagesCount = await _context.messages
                  .Where(a => a.IsRead == false && a.ReceiverId.Equals(user.Id)).CountAsync();

                HttpContext.Session.SetInt32("UnreadMessagesCount", unreadMessagesCount);
            }
           
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Reply(int messageId)
        {
            string user = await _context.messages.Where(a => a.Id == messageId).Select(a => a.ReceiverId).FirstOrDefaultAsync();
            ReplyViewModel model = new ReplyViewModel()
            {
                Receiver = await _context.customers.Where(a => a.Id.Equals(user)).Select(a => a.Email).FirstOrDefaultAsync()
            };
          
            return View(model);
        }

        public async Task<IActionResult> Read(int messageId)
        {
            Areas.Identity.Data.Message mess = await _context.messages.Where(a => a.Id == messageId).FirstOrDefaultAsync();
            mess.IsRead = true;
            await _context.SaveChangesAsync();

            var user = await _userManager.GetUserAsync(User);

            int unreadMessagesCount = await _context.messages
           .Where(a => a.IsRead == false && a.ReceiverId.Equals(user.Id)).CountAsync();

            HttpContext.Session.SetInt32("UnreadMessagesCount", unreadMessagesCount);

            ReadViewModel model = new ReadViewModel()
            {
                Body = mess.Body,
                Subject = mess.Subject,
                Sender = await _context.customers.Where(a => a.Id.Equals(mess.SenderId)).Select(a => a.Email).FirstOrDefaultAsync(),
                SenderId = mess.SenderId,
                Receiver = mess.ReceiverId
            };

            return View(model);
        }

        public async Task<ListOfMessageViewModel> sortlist(int option, string search, ListOfMessageViewModel model)
        {
            ListOfMessageViewModel mod = new ListOfMessageViewModel();
            mod.ListOfMessages = new List<MessageViewModel>();
            
            switch (option)
            {
                case 1:
               mod.ListOfMessages = model.ListOfMessages.OrderBy(a => a.SentDate).ToList();
                    break;
                case 2:
                    mod.ListOfMessages = model.ListOfMessages.Where(a => a.IsRead == false).ToList();
                    break;

                case 3:
                    // kategori
                    mod.ListOfMessages = model.ListOfMessages.Where(a => a.IsRead == true).ToList();
                    break;

                case 4:
                    // kategori
                    mod.ListOfMessages = model.ListOfMessages.OrderByDescending(a => a.SentDate).ToList();
                    break;
                   
                default:
                    mod = model;
                    break;
            }
            if ( search is not null) 
            {
                var user = await _context.customers.Where(a => a.Email.Equals(search)).FirstOrDefaultAsync();
                if (user is null)
                {
                    mod.ListOfMessages = new List<MessageViewModel>();
                }
                else 
                {
                    mod.ListOfMessages = model.ListOfMessages.Where(a => a.Sender.Equals(user.Email)).ToList();
                }
                  
                
                
            }

                
           
            return  mod;
        }
    }
}
