using Platinum.Areas.Identity.Data;
using Microsoft.Build.Framework;
using System.Security.Principal;

namespace Platinum.Models
{
    public class CheckLoanTermsViewModel
    {

        public string? UserName { get; set; }
        [Required]
        public decimal Amount { get; set; }

        public decimal? InterestRate { get; set; }

        public decimal? Interest { get; set; }

        public int Months { get; set; }

        public List<BankAccount>? Accounts { get; set; } = new List<BankAccount>();
        public string? ChoosenAccount { get; set; }

        public int loanTerm { get; set; }
    }

}
