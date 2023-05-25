using Microsoft.Build.Framework;

namespace Platinum.Areas.Identity.Data
{
    public class CardAccount
    {
        [Required]
        public int BankAccountId { get; set; }
        [Required]
        public BankAccount BankAccount { get; set; }
        [Required]
        public int CardId { get; set; }
        [Required]
        public Card Card { get; set; }
    }

}
