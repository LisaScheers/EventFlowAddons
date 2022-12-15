using EventFlow;
using EventFlow.Extensions;
using LisaScheers.EventFlowAddons.CosmosDB.SnapshotStores;
using Microsoft.Extensions.DependencyInjection;

namespace LisaScheers.EventFlowAddons.CosmosDB.Extensions
{

    public static class EventFlowOptionsSnapshotExtensions
    {
        public static IEventFlowOptions UseCosmosDbSnapshotStore(
            this IEventFlowOptions eventFlowOptions)
        {
            return eventFlowOptions
                .UseSnapshotPersistence<CosmosDbSnapshotPersistence>(ServiceLifetime.Transient);
        }
    }
}