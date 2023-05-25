using Platinum.Controllers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Platinum.Areas.Identity.Data
{
    public class Events
    {
        private readonly AssetApiController _assetApiController;
        private readonly IServiceProvider _serviceProvider;
        //private readonly TransactionController _transactionController;



        public Events(AssetApiController assetApiController, IServiceProvider serviceProvider)
        {
            _assetApiController = assetApiController;
           _serviceProvider= serviceProvider;
            
        }


        // kan lägga in felmeddelande men vågar ej testa att kalla på apin
        // om det skulle kosta
        public async Task  FinanceEvents()
        {
            //await _assetApiController.CryptoCurrency();
            //await  _assetApiController.StocksApple();
            //await _assetApiController.StocksMicroSoft();
            //await _assetApiController.Indexes();
            
        }

        public async Task PayRentEvent()
 {
            using var scope = _serviceProvider.CreateScope();
            var rentPayer = scope.ServiceProvider.GetRequiredService
                <TransactionController>();
            await rentPayer.PayRent();
        }



    }
}
