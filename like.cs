using System;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Newtonsoft.Json;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;


namespace JJ.Function
{
    public class ArticleItem
    {
        public string id { get; set; }
        public string articleid { get; set; }
        public int voteCount { get; set; }
    }

    public class VoteItem
    {
        public string id { get; set; }
        public string articleid { get; set; }
    }

    public static class like
    {
        // Save like into votes colletion
        // sample: http://localhost:7071/api/like?articleId=1
        // sample: while ($i -ne 30) { curl https://jjfunctionevents.azurewebsites.net/api/like?articleId=1 ; $i++ }
        [FunctionName("like")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            [CosmosDB(
                databaseName: "jjdb",
                collectionName: "votes",
                ConnectionStringSetting = "jjcosmos_DOCUMENTDB")]
                IAsyncCollector<VoteItem> votesOut,
            ILogger log)
        {
            log.LogInformation("Article like triggered...");

            string articleId = req.Query["articleId"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            articleId = articleId ?? data?.articleId;

            VoteItem newVoteItem = new VoteItem { articleid = articleId};
            await votesOut.AddAsync(newVoteItem);
       
            string responseMessage = "like accepted";
            return new OkObjectResult(responseMessage);
        }
    }
}
