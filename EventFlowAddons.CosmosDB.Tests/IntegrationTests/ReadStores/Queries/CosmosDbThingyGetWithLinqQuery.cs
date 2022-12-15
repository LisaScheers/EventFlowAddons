
using System.Linq;
using EventFlow.Queries;
using LisaScheers.EventFlowAddons.CosmosDB.Tests.IntegrationTests.ReadStores.ReadModels;

namespace LisaScheers.EventFlowAddons.CosmosDB.Tests.IntegrationTests.ReadStores.Queries
{

    public class CosmosDbThingyGetWithLinqQuery : IQuery<IOrderedQueryable<CosmosDbThingyReadModel>>
    {
    }
}