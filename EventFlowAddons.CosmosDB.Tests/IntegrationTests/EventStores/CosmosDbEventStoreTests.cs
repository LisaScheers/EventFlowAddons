using EventFlow;
using EventFlow.TestHelpers;
using EventFlow.TestHelpers.Suites;
using LisaScheers.EventFlowAddons.CosmosDB.EventStore;
using LisaScheers.EventFlowAddons.CosmosDB.Extensions;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;

namespace LisaScheers.EventFlowAddons.CosmosDB.Tests.IntegrationTests.EventStores;

[Category(Categories.Integration)]
[TestFixture]
public class CosmosDbEventStoreTests : TestSuiteForEventStore
{
    [TearDown]
    public void TearDown()
    {
        _cosmosClient.GetDatabase("EventFlowTest").DeleteAsync().Wait();
    }

    private CosmosClient _cosmosClient;

    protected override IServiceProvider Configure(IEventFlowOptions eventFlowOptions)
    {
        _cosmosClient = new CosmosClient("https://localhost:8081",
            "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==");

        _cosmosClient.CreateDatabaseIfNotExistsAsync("EventFlowTest").Wait();

        eventFlowOptions
            .ConfigureCosmosDb(() => _cosmosClient.GetDatabase("EventFlowTest"))
            .UseCosmosDbEventStore();


        var serviceProvider = base.Configure(eventFlowOptions);

        var eventPersistenceInitializer = serviceProvider.GetService<ICosmosDbEventPersistenceInitializer>();
        eventPersistenceInitializer.InitializeAsync().Wait();

        return serviceProvider;
    }
}