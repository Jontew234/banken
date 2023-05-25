namespace Platinum.Models
{
    public class DisplayAllAccountsViewModel
    {
        public IList<AccountViewModel> Accounts { get; set; }

        public InternalTransactionViewModel transactionViewModel { get; set; }
    }
}
