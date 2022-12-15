
using System;
using EventFlow;
using EventFlow.TestHelpers;
using EventFlow.TestHelpers.Suites;
using EventFlowAddons.CosmosDB.EventStore;
using EventFlowAddons.CosmosDB.Extensions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Microsoft.Azure.Cosmos;


namespace EventFlowAddons.CosmosDB.Tests.IntegrationTests.EventStores
{
	[Category(Categories.Integration)]
	[TestFixture]
    public class CosmosDbEventStoreTests : TestSuiteForEventStore
	{
		
		protected override IServiceProvider Configure(IEventFlowOptions eventFlowOptions)
		{
		    
		    eventFlowOptions
			    .ConfigureCosmosDb(() =>
			    {
				    var cosmosClient = new CosmosClient("https://localhost:8081", "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==");
				    return cosmosClient.GetDatabase("cosmos-test");
			    })
			    .UseCosmosDbEventStore();
		    
            
		    var serviceProvider = base.Configure(eventFlowOptions);
		    
		    var eventPersistenceInitializer = serviceProvider.GetService<ICosmosDbEventPersistenceInitializer>();
            eventPersistenceInitializer.InitializeAsync().Wait();

            return serviceProvider;
		}

        [TearDown]
		public void TearDown()
		{
		}
	}
}
