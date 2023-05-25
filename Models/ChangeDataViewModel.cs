using System.ComponentModel.DataAnnotations;

namespace Platinum.Models
{
    public class ChangeDataViewModel
    {

        public string FirstName { get; set; }

        [Required(ErrorMessage = "You need to add a lasttname")]
        [StringLength(100, ErrorMessage = "The name is to long")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "You need to add a phonenumber")]
        [RegularExpression("^(((\\+46)|0)7[0236])?[- ]?[0-9]{7,8}$",
            ErrorMessage = "Invalid number")]
        public string PhoneNumber { get; set; }
        [Required(ErrorMessage = "You need to add a adress")]
        public string Address { get; set; }
    }
}
