using System.ComponentModel.DataAnnotations;

namespace Platinum.Areas.Identity.Data
{
    public class LoanTerms
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }

        [Required]
        [Range(0, 1, ErrorMessage = "Value must be between 0 and 1.")]
        public decimal Terms { get; set; }

        [Required]
        public decimal Interestrate { get; set; }

        [Required]
        public bool IsActive { get; set; }
    }
}
