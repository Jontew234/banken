namespace Platinum.Areas.Identity.Data
{
    public class CurrentlyOwnedAssets
    {
        public int InvestmentAccountId { get; set; }
        public InvestmentAccount InvestmentAccount { get; set; }

        public int AssetId { get; set; }

        public Asset Asset { get; set; }

        public decimal Quantity { get; set; }

    }
}
