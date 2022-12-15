using EventFlow;
using EventFlow.Extensions;
using EventFlowAddons.CosmosDB.SnapsotStores;
using Microsoft.Extensions.DependencyInjection;

namespace EventFlowAddons.CosmosDB.Extensions;

public static class EventFlowOptionsSnapshotExtensions
{
    public static IEventFlowOptions UseCosmosDbSnapshotStore(
        this IEventFlowOptions eventFlowOptions)
    {
        return eventFlowOptions
            .UseSnapshotPersistence<CosmosDbSnapshotPersistence>(ServiceLifetime.Transient);
    }
}
