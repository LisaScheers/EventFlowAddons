
using System;
using EventFlow.Configuration;
using EventFlow.TestHelpers;
using EventFlow.TestHelpers.Suites;
using EventFlowAddons.CosmosDB.Extensions;
using Microsoft.Azure.Cosmos;
using NUnit.Framework;

namespace EventFlow.CosmosDB.Tests.IntegrationTests.SnapshotStores
{
    [Category(Categories.Integration)]
    public class CosmosDBSnapshotStoreTests : TestSuiteForSnapshotStore
    {
      

        protected override IServiceProvider Configure(IEventFlowOptions eventFlowOptions)
        {
           

            eventFlowOptions
                .ConfigureCosmosDb(() =>
                {
                    var client = new CosmosClient("https://localhost:8081","C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==");
                    return client.GetDatabase("cosmos-test");
                })
                .UseCosmosDbSnapshotStore();

            var serviceProvider = base.Configure(eventFlowOptions);

            return serviceProvider;
        }

       
    }
}