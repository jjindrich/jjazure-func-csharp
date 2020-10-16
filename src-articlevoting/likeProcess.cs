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
using Microsoft.Azure.Documents;

namespace JJ.Function
{
    // Started when like saved to votes collection
    // increase vote for article
    public static class likeProcess
    {
        [FunctionName("likeProcess")]
        public static async Task Run(
        [CosmosDBTrigger(
            databaseName: "jjdb",
            collectionName: "votes",
            ConnectionStringSetting = "jjcosmos_DOCUMENTDB",
            LeaseCollectionName = "leases", 
            CreateLeaseCollectionIfNotExists = true)] IReadOnlyList<Document> inputDocuments,
        [CosmosDB(
            databaseName: "jjdb",
            collectionName: "articles",
            ConnectionStringSetting = "jjcosmos_DOCUMENTDB")] DocumentClient client,
        [CosmosDB(
            databaseName: "jjdb",
            collectionName: "articles",
            ConnectionStringSetting = "jjcosmos_DOCUMENTDB")]
            ICollector<ArticleItem> articlesOut,
        ILogger log)
        {            
            if (inputDocuments != null && inputDocuments.Count > 0)
            {
                log.LogInformation("Like process triggered, received document count: " + inputDocuments.Count.ToString());

                foreach (Document input in inputDocuments)
                {
                    string articleId = input.GetPropertyValue<string>("articleid");
                    log.LogInformation("Processing articleId " + articleId);

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
                            log.LogInformation("Processed articleId " + articleId + " with voteCount " + result.voteCount.ToString());
                        }
                    }
                }
            }
        }
    }
}
