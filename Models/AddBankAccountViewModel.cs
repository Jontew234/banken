using Platinum.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;

namespace Platinum.Models
{
    public class AddBankAccountViewModel
    {
        public int Id { get; set; }
        public string AccountNumber { get; set; }
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "You dont have enough money")]

        public decimal Balance { get; set; }

        [Required]
        [EnumDataType(typeof(AccountType))]
        public string AccountType { get; set; }
        public bool HasCard { get; set; }
    }
}
