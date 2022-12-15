using EventFlow.TestHelpers;
using EventFlow.TestHelpers.Suites;
using EventFlowAddons.CosmosDB.Extensions;
using Microsoft.Azure.Cosmos;

namespace EventFlow.CosmosDB.Tests.IntegrationTests.SnapshotStores;

[Category(Categories.Integration)]
public class CosmosDBSnapshotStoreTests : TestSuiteForSnapshotStore
{
    private CosmosClient _cosmosClient;

    protected override IServiceProvider Configure(IEventFlowOptions eventFlowOptions)
    {
        _cosmosClient = new CosmosClient("https://localhost:8081",
            "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==");

        _cosmosClient.CreateDatabaseIfNotExistsAsync("EventFlowTest").Wait();
        eventFlowOptions
            .ConfigureCosmosDb(() => _cosmosClient.GetDatabase("EventFlowTest"))
            .UseCosmosDbSnapshotStore();

        var serviceProvider = base.Configure(eventFlowOptions);

        return serviceProvider;
    }

    [TearDown]
    public void TearDown()
    {
        _cosmosClient.GetDatabase("EventFlowTest").DeleteAsync().Wait();
    }
}