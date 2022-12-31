using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventFlow.Queries;
using EventFlow.TestHelpers.Aggregates.Entities;
using EventFlow.TestHelpers.Aggregates.Queries;
using LisaScheers.EventFlowAddons.Elastic.ReadStore;
using LisaScheers.EventFlowAddons.Elastic.Tests.IntegrationTests.ReadStores.ReadModels;

namespace LisaScheers.EventFlowAddons.Elastic.Tests.IntegrationTests.ReadStores.QueryHandlers
{
    public class
        ElasticThingyGetMessagesQueryHandler : IQueryHandler<ThingyGetMessagesQuery,
            IReadOnlyCollection<ThingyMessage>>
    {
        private readonly IElasticSearchReadModelStore<ElasticThingyMessageReadModel> _readStore;

        public ElasticThingyGetMessagesQueryHandler(
            IElasticSearchReadModelStore<ElasticThingyMessageReadModel> mongeReadStore)
        {
            _readStore = mongeReadStore;
        }

        public async Task<IReadOnlyCollection<ThingyMessage>> ExecuteQueryAsync(ThingyGetMessagesQuery query,
            CancellationToken cancellationToken)
        {
     

            var thingyId = query.ThingyId.ToString();


            var readmodels = _readStore.GetWithQueryAsync(c =>
                c.Query(q => q.MatchPhrase(match => match.Query(thingyId).Field(f => f.ThingyId))));
            var list = readmodels.Result.Documents.ToList();
            var messages = list.Select(m => m.ToThingyMessage()).ToList();
            return messages;
        }
    }
}