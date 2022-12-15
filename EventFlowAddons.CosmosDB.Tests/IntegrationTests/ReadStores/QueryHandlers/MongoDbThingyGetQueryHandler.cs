using EventFlow.CosmosDB.Tests.IntegrationTests.ReadStores.ReadModels;
using EventFlow.Queries;
using EventFlow.TestHelpers.Aggregates;
using EventFlow.TestHelpers.Aggregates.Queries;
using EventFlowAddons.CosmosDB.ReadStore;

namespace EventFlowAddons.CosmosDB.Tests.IntegrationTests.ReadStores.QueryHandlers;

public class CosmosDbThingyGetQueryHandler : IQueryHandler<ThingyGetQuery, Thingy>
{
    private readonly ICosmosDbReadModelStore<CosmosDbThingyReadModel> _readStore;

    public CosmosDbThingyGetQueryHandler(
        ICosmosDbReadModelStore<CosmosDbThingyReadModel> mongeReadStore)
    {
        _readStore = mongeReadStore;
    }

    public async Task<Thingy> ExecuteQueryAsync(ThingyGetQuery query, CancellationToken cancellationToken)
    {
        var thingyId = query.ThingyId.ToString();
        var thing = await _readStore.GetAsync(thingyId, cancellationToken);
        return thing.ReadModel?.ToThingy();
    }
}