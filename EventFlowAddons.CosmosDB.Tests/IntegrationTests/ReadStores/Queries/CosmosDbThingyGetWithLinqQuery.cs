using EventFlow.CosmosDB.Tests.IntegrationTests.ReadStores.ReadModels;
using EventFlow.Queries;

namespace LisaScheers.EventFlowAddons.CosmosDB.Tests.IntegrationTests.ReadStores.Queries;

public class CosmosDbThingyGetWithLinqQuery : IQuery<IOrderedQueryable<CosmosDbThingyReadModel>>
{
}