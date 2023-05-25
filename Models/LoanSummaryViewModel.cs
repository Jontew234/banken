using Platinum.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;

namespace Platinum.Models
{
    public class LoanSummaryViewModel
    {
        [Range(1, double.MaxValue)]
        public decimal? Amount { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [RegularExpression(@"^\d{4}-\d{6}-\d{4}$",
    ErrorMessage = "The account number must be in the format xxxx-xxxxxx-xxxx")]
        public string AccountNumber { get; set; }

        public decimal? YearlyInterest { get; set; }

        public decimal Interestrate { get; set; }

        public decimal LeftToPay { get; set; }

        public int Id { get; set; }

        public List<BankAccount>? Accounts { get; set; } 
    }
}
