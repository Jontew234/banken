
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platinum.Areas.Identity.Data
{
    public class Card
    {
        public int Id { get; set; }
        [RegularExpression(@"^[0-9]{16}$", ErrorMessage = "Invalid card number")]
        [Required(ErrorMessage = "Card number is required")]
        public string CardNumber { get; set; }

        [Range(1, 12, ErrorMessage = "Invalid Month")]
        [Required(ErrorMessage = "Expiration Month is required")]
        public int ExpirationMonth { get; set; }

        [Range(2022, 2050, ErrorMessage = "Invalid Year")]
        [Required(ErrorMessage = "Expiration Year is required")]
        public int ExpirationYear { get; set; }

        [RegularExpression(@"^[0-9]{3}$", ErrorMessage = "Invalid CVC")]
        [Required(ErrorMessage = "CVV is required")]
        public string CVV { get; set; }

        [Required]
        public bool OnlinePurchase { get; set; }

        public string CustomerId { get; set; }
        [Required]
        public Customer? Customer { get; set; }

        [Required]
        public bool Active { get; set; }
        // kan tas bort när many to many är klart
        //public int? BankAccountId { get; set; }
        
        //[Required(ErrorMessage = "the card must belong to an bankaccount")]
        //public BankAccount BankAccount { get; set; }

        public ICollection<CardAccount> CardAccounts { get; set; }
    }
}
