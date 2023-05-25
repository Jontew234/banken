namespace Platinum.Models
{
    public class TransactionSummaryViewModel
    {

        public string AccountNumber { get; set; }
        public bool BuyOrsell { get; set; }

        public decimal Amount { get; set; }

        public string Asset { get; set; }

        public decimal Sum { get; set; }

        public DateTime time { get; set; }
    }
}
