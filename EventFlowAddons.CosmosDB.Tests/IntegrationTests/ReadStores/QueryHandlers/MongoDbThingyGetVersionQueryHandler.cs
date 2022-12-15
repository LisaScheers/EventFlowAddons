
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventFlow.CosmosDB.Tests.IntegrationTests.ReadStores.ReadModels;
using EventFlow.Queries;
using EventFlow.TestHelpers.Aggregates;
using EventFlow.TestHelpers.Aggregates.Queries;
using EventFlowAddons.CosmosDB.ReadStore;

namespace EventFlowAddons.CosmosDB.Tests.IntegrationTests.ReadStores.QueryHandlers
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