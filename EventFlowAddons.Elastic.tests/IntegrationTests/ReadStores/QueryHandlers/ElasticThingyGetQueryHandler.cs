using System.Threading;
using System.Threading.Tasks;
using EventFlow.Queries;
using EventFlow.TestHelpers.Aggregates;
using EventFlow.TestHelpers.Aggregates.Queries;
using LisaScheers.EventFlowAddons.Elastic.ReadStore;
using LisaScheers.EventFlowAddons.Elastic.Tests.IntegrationTests.ReadStores.ReadModels;

namespace LisaScheers.EventFlowAddons.Elastic.Tests.IntegrationTests.ReadStores.QueryHandlers
{

    public class ElasticThingyGetQueryHandler : IQueryHandler<ThingyGetQuery, Thingy>
    {
        private readonly IElasticSearchReadModelStore<ElasticThingyReadModel> _readStore;

        public ElasticThingyGetQueryHandler(
            IElasticSearchReadModelStore<ElasticThingyReadModel> mongeReadStore)
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
}