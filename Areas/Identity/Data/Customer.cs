using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Platinum.Areas.Identity.Data;

// Add profile data for application users by adding properties to the Customer class
public class Customer : IdentityUser
{

    [Required(ErrorMessage = "You need to add a firstname")]
    [StringLength(100, ErrorMessage = "The name is to long")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "You need to add a lasttname")]
    [StringLength(100, ErrorMessage = "The name is to long")]
    public string LastName { get; set; }



    [Required(ErrorMessage = "You need to add a phonenumber")]
    [RegularExpression("^(((\\+46)|0)7[0236])?[- ]?[0-9]{7,8}$",
        ErrorMessage = "Invalid number")]
    public string PhoneNumber { get; set; }
    [Required(ErrorMessage = "You need to add a adress")]
    public string Address { get; set; }
    public ICollection<BankAccount>? BankAccounts { get; set; }
    public ICollection<Loan>? Loans { get; }
    public ICollection<Message>? SentMessages { get; }

    public ICollection<Message>? ReceivedMessages { get; }

    public ICollection<Card>? Cards { get; }

    public ICollection<Invoice> Invoices { get; set; }
}

