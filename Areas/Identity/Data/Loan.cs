using Platinum.Areas.Identity.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platinum.Areas.Identity.Data
{
    public class Loan
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [Range(1, double.MaxValue)]
        public decimal? Amount { get; set; }
        [Required]
        public int Months { get; set; } // in months
        [Required]
        [Range(1, 50)]
        public decimal InterestRate { get; set; }
        [Required]
        public DateTime StartDate { get; set; } = DateTime.Now;

        [Required]
        public decimal LeftToPay { get; set; }
        [Required]
        public DateTime EndDate { get; set; }


        [Required]
        [RegularExpression(@"^\d{4}-\d{6}-\d{4}$",
            ErrorMessage = "The account number must be in the format xxxx-xxxxxx-xxxx")]
        public string AccountNumber { get; set; }
        [Required]
        public string CustomerId { get; set; }
        public virtual Customer User { get; set; }

        [ForeignKey("Terms")]
        public int LoanTermsId { get; set; }

        public virtual LoanTerms Terms { get; set; }

        public int BankAccountId { get; set; }
        public int BankAccIdTo { get; set; }

        public virtual BankAccount ToAccount { get; set; }
        public virtual BankAccount BankAccount { get; set; }

        public virtual ICollection<Invoice> Invoice { get; set; }





    }
}