using System.Threading;
using System.Threading.Tasks;
using EventFlow.Queries;
using EventFlow.TestHelpers.Aggregates.Queries;
using LisaScheers.EventFlowAddons.Elastic.ReadStore;
using LisaScheers.EventFlowAddons.Elastic.Tests.IntegrationTests.ReadStores.ReadModels;

namespace LisaScheers.EventFlowAddons.Elastic.Tests.IntegrationTests.ReadStores.QueryHandlers
{

    public class ElasticThingyGetVersionQueryHandler : IQueryHandler<ThingyGetVersionQuery, long?>
    {
        private readonly IElasticSearchReadModelStore<ElasticThingyReadModel> _readStore;

        public ElasticThingyGetVersionQueryHandler(
            IElasticSearchReadModelStore<ElasticThingyReadModel> mongeReadStore)
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