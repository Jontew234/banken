using System.ComponentModel.DataAnnotations;

namespace Platinum.Models
{
    public class EditCardViewModel
    {
        public int Id { get; set; }
        //[RegularExpression(@"^(?:4[0-9]{12}(?:[0-9]{3})?|[25][1-7][0-9]{14}|6(?:011|5[0-9][0-9])[0-9]{12}|3[47][0-9]{13}|3(?:0[0-5]|[68][0-9])[0-9]{11}|(?:2131|1800|35\d{3})\d{11})$", ErrorMessage = "Invalid card number")]
        //[Required(ErrorMessage = "Card number is required")]
        public string CardNumber { get; set; }
       // [Required]
        public bool OnlinePurchase { get; set; }


       // [Required]
        public bool Active { get; set; }
    }
}
