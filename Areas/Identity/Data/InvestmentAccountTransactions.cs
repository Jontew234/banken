namespace Platinum.Areas.Identity.Data
{
    // här ser vi alla transaktioner för ett investmentkonto
    public class InvestmentAccountTransactions
    {
        public int Id { get; set; }
        public int InvestmentAccountId { get; set; }
        public InvestmentAccount InvestmentAccount { get; set; }

        public int AssetId { get; set; }
        public Asset Asset { get; set; }

        public decimal Quantity { get; set; }

        public DateTime time  { get; set; }

        public bool BuyAndSell { get; set; }

       
      
    }
}
