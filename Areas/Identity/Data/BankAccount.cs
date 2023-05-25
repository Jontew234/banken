
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Platinum.Areas.Identity.Data
{

    public class BankAccount
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [RegularExpression(@"^\d{4}-\d{6}-\d{4}$",
            ErrorMessage = "The account number must be in the format xxxx-xxxxxx-xxxx")]
        public string AccountNumber { get; set; }
        [Required]
        [Range(1, double.MaxValue, ErrorMessage = "You dont have enough money")]

        public decimal Balance { get; set; }

        [Required]
        [EnumDataType(typeof(AccountType))]
        public string AccountType { get; set; }

        [Required]
        public bool IsActive { get; set; }

        [Required]
        public string CustomerId { get; set; }
        [Required]
        public Customer Customer { get; set; }

        public ICollection<CardAccount> CardAccounts { get; set; }

     
        public ICollection<Loan> Loans { get; set; }

       
        public ICollection<Loan> PayTo { get; set; }
    }


}

