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
        [FunctionName("like")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            /*
            JJ: nefunguje mapovani na query string
            error: Microsoft.Azure.WebJobs.Host: Exception binding parameter 'articlesItems'. Microsoft.Azure.WebJobs.Host: Error while accessing 'articleId': property doesn't exist.
            [CosmosDB(                
                databaseName: "jjdb",
                collectionName: "articles",
                ConnectionStringSetting = "jjcosmos_DOCUMENTDB",
                SqlQuery = "select * from articles r where r.articleid = {Query.articleId}")]
                IEnumerable<ArticleItem> articlesItems,
            */
            [CosmosDB(
                databaseName: "jjdb",
                collectionName: "articles",
                ConnectionStringSetting = "jjcosmos_DOCUMENTDB")] DocumentClient client,
            [CosmosDB(
                databaseName: "jjdb",
                collectionName: "articles",
                ConnectionStringSetting = "jjcosmos_DOCUMENTDB")]
                ICollector<ArticleItem> articlesOut,
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
            
            Uri collectionUri = UriFactory.CreateDocumentCollectionUri("jjdb", "articles");
            IDocumentQuery<ArticleItem> query = client.CreateDocumentQuery<ArticleItem>(collectionUri)
                .Where(p => p.articleid == articleId)
                .AsDocumentQuery();
 
            while (query.HasMoreResults)
            {
                foreach (ArticleItem result in await query.ExecuteNextAsync())
                {
                    log.LogInformation(result.voteCount.ToString());
                    result.voteCount++;
                    articlesOut.Add(result);

                    VoteItem newVoteItem = new VoteItem { articleid = result.articleid};
                    await votesOut.AddAsync(newVoteItem);
                }
            }

            string responseMessage = "done";
            return new OkObjectResult(responseMessage);
        }
    }
}
