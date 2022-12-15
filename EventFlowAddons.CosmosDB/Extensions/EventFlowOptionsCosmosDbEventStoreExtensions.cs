using EventFlow;
using EventFlow.Extensions;
using EventFlowAddons.CosmosDB.EventStore;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EventFlowAddons.CosmosDB.Extensions;

public static class EventFlowOptionsCosmosDbEventStoreExtensions
{
    public static IEventFlowOptions UseCosmosDbEventStore(this IEventFlowOptions eventFlowOptions)
    {

        eventFlowOptions.UseEventPersistence<CosmosDbEventPersistence>();
        eventFlowOptions.ServiceCollection
            .TryAddTransient<ICosmosDbEventPersistenceInitializer, CosmosDbEventPersistenceInitializer>();

        return eventFlowOptions;
    }
}