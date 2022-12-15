using EventFlow.CosmosDB.Tests.IntegrationTests.ReadStores.ReadModels;
using EventFlow.Queries;
using EventFlowAddons.CosmosDB.ReadStore;
using EventFlowAddons.CosmosDB.Tests.IntegrationTests.ReadStores.Queries;

namespace EventFlowAddons.CosmosDB.Tests.IntegrationTests.ReadStores.QueryHandlers;

public class
    CosmosDbThingyGetWithLinqQueryHandler : IQueryHandler<CosmosDbThingyGetWithLinqQuery,
        IOrderedQueryable<CosmosDbThingyReadModel>>
{
    private readonly ICosmosDbReadModelStore<CosmosDbThingyReadModel> _readStore;

    public CosmosDbThingyGetWithLinqQueryHandler(
        ICosmosDbReadModelStore<CosmosDbThingyReadModel> mongeReadStore)
    {
        _readStore = mongeReadStore;
    }

    public Task<IOrderedQueryable<CosmosDbThingyReadModel>> ExecuteQueryAsync(CosmosDbThingyGetWithLinqQuery query,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(_readStore.GetItemLinqQueryable());
    }
}