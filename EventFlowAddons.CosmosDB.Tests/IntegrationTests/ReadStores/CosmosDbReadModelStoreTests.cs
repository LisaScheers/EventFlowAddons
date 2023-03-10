using System;
using System.Linq;
using System.Threading.Tasks;
using EventFlow;
using EventFlow.Extensions;
using EventFlow.TestHelpers;
using EventFlow.TestHelpers.Aggregates;
using EventFlow.TestHelpers.Aggregates.Entities;
using EventFlow.TestHelpers.Extensions;
using EventFlow.TestHelpers.Suites;
using LisaScheers.EventFlowAddons.CosmosDB.Extensions;
using LisaScheers.EventFlowAddons.CosmosDB.ReadStore;
using LisaScheers.EventFlowAddons.CosmosDB.Tests.IntegrationTests.ReadStores.Queries;
using LisaScheers.EventFlowAddons.CosmosDB.Tests.IntegrationTests.ReadStores.QueryHandlers;
using FluentAssertions;
using LisaScheers.EventFlowAddons.CosmosDB.Tests.IntegrationTests.ReadStores.ReadModels;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace LisaScheers.EventFlowAddons.CosmosDB.Tests.IntegrationTests.ReadStores
{

    [Category(Categories.Integration)]
    [TestFixture]
    public class CosmosDbReadModelStoreTests : TestSuiteForReadModelStore
    {
        [TearDown]
        public void TearDown()
        {
            _cosmosClient.GetDatabase("EventFlowTest").DeleteAsync().Wait();
        }

        protected override Type ReadModelType { get; } = typeof(CosmosDbThingyReadModel);

        private CosmosClient _cosmosClient;

        protected override IServiceProvider Configure(IEventFlowOptions eventFlowOptions)
        {
            _cosmosClient = new CosmosClient("https://localhost:8081",
                "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==");

            _cosmosClient.CreateDatabaseIfNotExistsAsync("EventFlowTest").Wait();

            var resolver = eventFlowOptions
                .RegisterServices(sr => sr.AddTransient(typeof(ThingyMessageLocator)))
                .ConfigureCosmosDb(() => _cosmosClient.GetDatabase("EventFlowTest"))
                .UseCosmosDbReadModel<CosmosDbThingyReadModel>()
                .UseCosmosDbReadModel<CosmosDbThingyMessageReadModel, ThingyMessageLocator>()
                .AddQueryHandlers(
                    typeof(CosmosDbThingyGetQueryHandler),
                    typeof(CosmosDbThingyGetVersionQueryHandler),
                    typeof(CosmosDbThingyGetMessagesQueryHandler),
                    typeof(CosmosDbThingyGetWithLinqQueryHandler)
                );

            var serviceProvider = base.Configure(eventFlowOptions);

            var a = serviceProvider.GetService<ICosmosDbReadModelStore<CosmosDbThingyReadModel>>();
            var b = serviceProvider.GetService<ICosmosDbReadModelStore<CosmosDbThingyMessageReadModel>>();
            a.InitDatabaseAsync().Wait();
            b.InitDatabaseAsync().Wait();

            return serviceProvider;
        }

        [Test]
        public async Task AsQueryableShouldNotBeEmpty()
        {
            var id = ThingyId.New;

            await PublishPingCommandsAsync(id, 1).ConfigureAwait(false);

            var result = await QueryProcessor.ProcessAsync(new CosmosDbThingyGetWithLinqQuery()).ConfigureAwait(false);

            result.ToList().Should().NotBeEmpty();
        }
    }
}