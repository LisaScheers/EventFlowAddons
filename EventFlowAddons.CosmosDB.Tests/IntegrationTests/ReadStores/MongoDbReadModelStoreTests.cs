
using EventFlow;
using EventFlow.Extensions;

using EventFlow.CosmosDB.Tests.IntegrationTests.ReadStores.ReadModels;
using EventFlow.TestHelpers;
using EventFlow.TestHelpers.Aggregates;
using EventFlow.TestHelpers.Aggregates.Entities;
using EventFlow.TestHelpers.Extensions;
using EventFlow.TestHelpers.Suites;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using EventFlowAddons.CosmosDB.Extensions;
using EventFlowAddons.CosmosDB.Tests.IntegrationTests.ReadStores.Queries;
using EventFlowAddons.CosmosDB.Tests.IntegrationTests.ReadStores.QueryHandlers;
using Microsoft.Azure.Cosmos;
using NUnit.Framework;

namespace EventFlowAddons.CosmosDB.Tests.IntegrationTests.ReadStores
{
    [Category(Categories.Integration)]
    [TestFixture]
    public class CosmosDbReadModelStoreTests : TestSuiteForReadModelStore
    {
        protected override Type ReadModelType { get; } = typeof(CosmosDbThingyReadModel);


        protected override IServiceProvider Configure(IEventFlowOptions eventFlowOptions)
        {

            var resolver = eventFlowOptions
                .RegisterServices(sr => sr.AddTransient(typeof(ThingyMessageLocator)))
                .ConfigureCosmosDb(() =>
                {
                    var client = new CosmosClient("https://localhost:8081",
                        "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==");
                    return client.GetDatabase("cosmos-test");
                })
                .UseCosmosDbReadModel<CosmosDbThingyReadModel>()
                .UseCosmosDbReadModel<CosmosDbThingyMessageReadModel, ThingyMessageLocator>()
                .AddQueryHandlers(
                    typeof(CosmosDbThingyGetQueryHandler),
                    typeof(CosmosDbThingyGetVersionQueryHandler),
                    typeof(CosmosDbThingyGetMessagesQueryHandler),
                    typeof(CosmosDbThingyGetWithLinqQueryHandler)
                );

            var serviceProvider = base.Configure(eventFlowOptions);

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