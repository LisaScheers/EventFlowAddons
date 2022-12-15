using System.Threading;
using System.Threading.Tasks;
using EventFlow.Queries;
using EventFlow.TestHelpers.Aggregates.Queries;
using LisaScheers.EventFlowAddons.CosmosDB.ReadStore;
using LisaScheers.EventFlowAddons.CosmosDB.Tests.IntegrationTests.ReadStores.ReadModels;

namespace LisaScheers.EventFlowAddons.CosmosDB.Tests.IntegrationTests.ReadStores.QueryHandlers
{

    public class CosmosDbThingyGetVersionQueryHandler : IQueryHandler<ThingyGetVersionQuery, long?>
    {
        private readonly ICosmosDbReadModelStore<CosmosDbThingyReadModel> _readStore;

        public CosmosDbThingyGetVersionQueryHandler(
            ICosmosDbReadModelStore<CosmosDbThingyReadModel> mongeReadStore)
        {
            _readStore = mongeReadStore;
        }

        public async Task<long?> ExecuteQueryAsync(ThingyGetVersionQuery query, CancellationToken cancellationToken)
        {
            var thingyId = query.ThingyId.ToString();
            var thing = await _readStore.GetAsync(thingyId, cancellationToken);
            return thing.Version;
        }
    }
}