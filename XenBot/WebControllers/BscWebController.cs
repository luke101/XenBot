using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace XenBot.WebControllers
{
    public class BscWebController : IWebController
    {
        public async Task<decimal> GetPriceAsync()
        {
            RestClient client1 = new RestClient();
            var request = new RestRequest("https://min-api.cryptocompare.com/data/price?fsym=BNB&tsyms=USD");
            var queryResult = await client1.GetAsync(request);
            
            if (queryResult == null || queryResult.IsSuccessful == false)
            {
                throw new Exception("Could not Load price");
            }

            var content = queryResult.Content;

            var jObject = JObject.Parse(content);

            var price = jObject["USD"].ToString();

            if (string.IsNullOrWhiteSpace(price))
            {
                throw new Exception("Could not Load price");
            }

            return Convert.ToDecimal(price);
        }
    }
}
