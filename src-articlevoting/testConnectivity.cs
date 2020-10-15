using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;

namespace Company.Function
{
   
/* sample pro test
{
    "name" :"Call azure server",
    "url": "http://jjdevv2addc.jjdev.local"
}
{
    "name" :"Call azure server",
    "url": "http://jjdevv2appw.jjdev.local"
}
{
    "name" :"Call onprem server",
    "url": "http://jjdevbr1web.jjdev.local"
}
*/
    public static class testConnectivity
    {
        [FunctionName("testConnectivity")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            string adresa = req.Query["url"];
            adresa = adresa ?? data?.url;
            adresa = adresa ?? "https://www.google.com";

            using (var client = new HttpClient())
            {
                log.LogInformation("Trying to connect...");
                log.LogInformation("url: {0}", adresa);
                client.BaseAddress = new Uri(adresa);
                var result = await client.GetAsync("");
                string resultContent = await result.Content.ReadAsStringAsync();
                name = resultContent;
                log.LogInformation("Result:" + resultContent);
            }

            return name != null
                ? (ActionResult)new OkObjectResult($"Hello, {name}")
                : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        }
    }
}
