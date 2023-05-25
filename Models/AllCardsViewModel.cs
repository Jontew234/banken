using System.ComponentModel.DataAnnotations;

namespace Platinum.Models
{
    public class AllCardsViewModel
    {
        public int Id { get; set; }
        [RegularExpression(@"^[0-9]{16}$", ErrorMessage = "Invalid card number")]
        [Required(ErrorMessage = "Card number is required")]
        public string CardNumber { get; set; }
        
        public int Account { get; set; }
    }
}
