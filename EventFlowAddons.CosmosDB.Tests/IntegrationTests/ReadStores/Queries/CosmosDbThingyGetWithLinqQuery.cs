
using System.Linq;
using EventFlow.CosmosDB.Tests.IntegrationTests.ReadStores.ReadModels;
using EventFlow.Queries;

namespace EventFlowAddons.CosmosDB.Tests.IntegrationTests.ReadStores.Queries
{
    public class CosmosDbThingyGetWithLinqQuery : IQuery<IOrderedQueryable<CosmosDbThingyReadModel>>
    {
        public CosmosDbThingyGetWithLinqQuery()
        {
        }
    }
}