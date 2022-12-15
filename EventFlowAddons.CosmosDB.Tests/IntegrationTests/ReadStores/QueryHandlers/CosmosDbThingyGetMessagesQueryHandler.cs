using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventFlow.Queries;
using EventFlow.TestHelpers.Aggregates.Entities;
using EventFlow.TestHelpers.Aggregates.Queries;
using LisaScheers.EventFlowAddons.CosmosDB.ReadStore;
using LisaScheers.EventFlowAddons.CosmosDB.Tests.IntegrationTests.ReadStores.ReadModels;

namespace LisaScheers.EventFlowAddons.CosmosDB.Tests.IntegrationTests.ReadStores.QueryHandlers
{

    public class
        CosmosDbThingyGetMessagesQueryHandler : IQueryHandler<ThingyGetMessagesQuery,
            IReadOnlyCollection<ThingyMessage>>
    {
        private readonly ICosmosDbReadModelStore<CosmosDbThingyMessageReadModel> _readStore;

        public CosmosDbThingyGetMessagesQueryHandler(
            ICosmosDbReadModelStore<CosmosDbThingyMessageReadModel> mongeReadStore)
        {
            _readStore = mongeReadStore;
        }

        public async Task<IReadOnlyCollection<ThingyMessage>> ExecuteQueryAsync(ThingyGetMessagesQuery query,
            CancellationToken cancellationToken)
        {
            var thingyId = query.ThingyId.ToString();
            var messages = _readStore.GetItemLinqQueryable().Where(x => x.ThingyId == thingyId).ToList()
                .Select(x => x.ToThingyMessage())
                .ToList();
            return messages;
        }
    }
}