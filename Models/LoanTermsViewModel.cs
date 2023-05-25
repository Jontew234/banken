using System.ComponentModel.DataAnnotations;

namespace Platinum.Models
{
    public class LoanTermsViewModel
    {
  
        public int Id { get; set; }
      
        public string Name { get; set; }

        [Range(0, 1, ErrorMessage = "Value must be between 0 and 1.")]
        public decimal Terms { get; set; }

       
        public decimal Interestrate { get; set; }

      
        public bool IsActive { get; set; }
    }
}
