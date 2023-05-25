using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platinum.Areas.Identity.Data
{
    public class Tran
    {
        
        public int Id { get; set; }
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Value must be greater than 0.")]
        public decimal Amount { get; set; }
        [Required]
        public DateTime Date { get; set; }

        [Required] 
        public string Category { get; set; }
        [Required]
      
        public int FromBankAccountId { get; set; }
        [Required]
        public int ToBankAccountId { get; set; }
        [Required]
        public BankAccount FromBankAccount { get; set; }
        [Required]
        public BankAccount ToBankAccount { get; set; }
    }
}
