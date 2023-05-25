using System.ComponentModel.DataAnnotations;

namespace Platinum.Models
{
    public class AccountViewModel
    {
        public int Id { get; set; }
        [Required]
        public string AccountNumber { get; set; }

        [Required]
        public decimal Balance { get; set; }

        [Required]
        public string AccountType { get; set; }

       
    }
}
