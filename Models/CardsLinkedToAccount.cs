using Platinum.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;

namespace Platinum.Models
{
    public class CardsLinkedToAccount
    {
        public int Id { get; set; }
        [Required]
        [RegularExpression(@"^\d{4}-\d{6}-\d{4}$",
            ErrorMessage = "The account number must be in the format xxxx-xxxxxx-xxxx")]
        public string AccountNumber { get; set; }

        [Required]
        [EnumDataType(typeof(AccountType))]
        public string AccountType { get; set; }

        public int CardId { get; set; }
    }
}
