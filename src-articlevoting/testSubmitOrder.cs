using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace JJ.Function
{
    public class JJOrderReq
    {
        public string id { get; set; }
        public string address { get; set; }
    }
    
    public class JJOrderDocument
    {
        public string uniqueId { get; set; }
        public string id { get; set; }
        public string address { get; set; }
    }

    // test: curl -X POST http://localhost/api/testSubmitOrder -H "Content-Type: application/json" -d '{ \"id\": \"1\", \"address\": \"CZ\" }'
    public static class SubmitOrder
    {
        [FunctionName("testSubmitOrder")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            [CosmosDB(
            databaseName: "jjdb",
            collectionName: "orders", CreateIfNotExists = true,
            ConnectionStringSetting = "jjcosmos_orders_DOCUMENTDB")]
            ICollector<JJOrderDocument> ordersOut,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string body = await req.ReadAsStringAsync();
            var order = JsonConvert.DeserializeObject<JJOrderReq>(body);

            // use Copilot to save the order to CosmosDB (ordersOut.Add)

            return new OkObjectResult("Order created.");
        }
    }
}
