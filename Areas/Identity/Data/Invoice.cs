
using System.ComponentModel.DataAnnotations;
using MessagePack;

namespace Platinum.Areas.Identity.Data
{
    public class Invoice
    {

     

        public int ID { get; set; }

        public decimal Amount { get; set; }

        public bool Payed { get; set; }

        public DateTime LastDayToPay { get; set; }

        public string CustomerId { get; set; }
        public Customer Customer { get; set; }

        public int LoansId { get; set; }
        public Loan Loan { get; set; }
    }
}
