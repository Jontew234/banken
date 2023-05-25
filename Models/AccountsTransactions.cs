using System.ComponentModel.DataAnnotations;

namespace Platinum.Models
{
    public class AccountsTransactions
    {
        public int Id { get; set; }
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Value must be greater than 0.")]
        public decimal Amount { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required]

        public string FromBankAccount { get; set; }
        [Required]
        public string ToBankAccount { get; set; }

        public bool toAccount { get; set; }

        [Required]
        public string Category { get; set; }
    }
}
