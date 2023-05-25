namespace Platinum.Models
{
    public class InvoiceViewModel
    {
        public decimal Amount { get; set; }

        public bool Payed { get; set; }

        public DateTime LastDayToPay { get; set; }


        public int LoansId { get; set; }

        public int InvoiceId { get; set; }
        public List<string> AccountNumbers { get; set; }
    }

}
