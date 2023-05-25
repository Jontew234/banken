using System.ComponentModel.DataAnnotations;

namespace Platinum.Models
{
    public class AddCardViewModel
    {
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
    }
}
