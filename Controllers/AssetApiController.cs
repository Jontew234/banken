using Platinum.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CodeFixes;
using Newtonsoft.Json;
using System;
using System.Text.Json;
using System.Xml.Linq;
using Platinum.Areas.Identity.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json.Linq;

namespace Platinum.Controllers
{
    public class AssetApiController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IServiceProvider _serviceProvider;

        public AssetApiController(IHttpClientFactory httpClientFactory, IServiceProvider serviceProvider)
        {
            _httpClientFactory = httpClientFactory;
            _serviceProvider = serviceProvider;
   


        }



        // alla calen med just for return är redundant tror jag är ju endast om man har något att spara
        public async Task StocksApple()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var investmentService = scope.ServiceProvider.GetRequiredService
                    <InvestmentController>();

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri
                   ("search?query=AAPL&language=en", UriKind.Relative),
                };

                //search? query = Apple & language = en
                //Microsoft NASDAQ
                var client = _httpClientFactory.CreateClient("Finance");
                using (var response = await client.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();
                    dynamic jsonObj = JsonConvert.DeserializeObject(body);

                    Asset model = new Asset(); ;
                    model.Name = jsonObj.data.stock[0].name;
                    model.Exchange = jsonObj.data.stock[0].exchange;
                    decimal pri = jsonObj.data.stock[0].price;
                    model.Price = decimal.Round(pri, 2);
                    model.Type = jsonObj.data.stock[0].type;
                    model.Risk = "6/7";
                    model.LastUpdated = DateTime.Now;
                    await investmentService.SaveAssetToDatabase(model);


                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
              
             

            }
        }

        // endast för att ta hem tillgångarna från apin
        public async Task StocksMicroSoft()
        {

            try
            {

                using var scope = _serviceProvider.CreateScope();
                var investmentService = scope.ServiceProvider.GetRequiredService
                    <InvestmentController>();

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri
                   ("search?query=Microsoft NASDAQ&language=en", UriKind.Relative),
                };

              
                var client = _httpClientFactory.CreateClient("Finance");
                using (var response = await client.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();
                    dynamic jsonObj = JsonConvert.DeserializeObject(body);

                    Asset model = new Asset(); ;
                    model.Name = jsonObj.data.stock[0].name;
                    model.Exchange = jsonObj.data.stock[0].exchange;
                    decimal pri = jsonObj.data.stock[0].price;
                    model.Price = decimal.Round(pri, 2);
                    model.Type = jsonObj.data.stock[0].type;
                    model.Risk = "6/7";
                    model.LastUpdated = DateTime.Now;
                    await investmentService.SaveAssetToDatabase(model);

                }
            }
            catch (Exception ex)
            {
               // fixa bättre hantering
                Console.WriteLine(ex.Message);
             
            }
        }

        public async Task Indexes()
        {
            try
            {
             

                using var scope = _serviceProvider.CreateScope();
                var investmentService = scope.ServiceProvider.GetRequiredService
                    <InvestmentController>();


                AllInvestments index = new AllInvestments();
                index.Indexes = new List<Asset>();
                var client = _httpClientFactory.CreateClient("Finance");
                var request = new HttpRequestMessage

                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri
                   ("search?query=Dow Johns&language=en", UriKind.Relative),
                };


                using (var response = await client.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();
                    dynamic jsonObj = JsonConvert.DeserializeObject(body);

                    for (int i = 0; i < 5; i++)
                    {
                        Asset model = new Asset();
                        model.Name = jsonObj.data.index[i].name;
                        model.Type = jsonObj.data.index[i].type;
                        decimal pri = jsonObj.data.index[i].price;
                        model.Price = decimal.Round(pri, 2);
                        model.Risk = "5/7";
                        model.LastUpdated = DateTime.Now;
                        model.Exchange = "Dow Johns";
                        index.Indexes.Add(model);
                    }
                 await  investmentService.SaveIndexAssetToDatabase(index);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            

            }

        }

        public async Task CryptoCurrency()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var investmentService = scope.ServiceProvider.GetRequiredService
                    <InvestmentController>();

                // byt ut denna
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,

                    RequestUri = new Uri
                   ("/coin/Qwsogvtv82FCd?referenceCurrencyUuid=yhjMzLPhuIDl&timePeriod=24h",
                   UriKind.Relative),
                };

                // skapa den för crypto
                var client = _httpClientFactory.CreateClient("Crypto");


                using (var response = await client.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();
                    dynamic jsonObj = JsonConvert.DeserializeObject(body);

                    // beroende på hur apin ser ut så får vii justera denna
                    Asset model = new Asset();
                    model.Name = jsonObj.data.coin.name;
                    model.Exchange = "Platina Bank";
                    decimal pri = jsonObj.data.coin.price;
                    model.Price = decimal.Round(pri, 2);
                    model.Type = "Crypto";
                    model.Risk = "7/7";
                    model.LastUpdated = DateTime.Now;
                    await investmentService.SaveAssetToDatabase(model);

                }
            }
            catch (HttpRequestException ex)
            {
                // borde logga eller något
                Console.WriteLine(ex.Message);

            }
        }
    }

     
}


