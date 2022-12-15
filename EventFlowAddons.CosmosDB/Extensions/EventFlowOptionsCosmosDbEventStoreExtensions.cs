using EventFlow;
using EventFlow.Extensions;
using LisaScheers.EventFlowAddons.CosmosDB.EventStore;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LisaScheers.EventFlowAddons.CosmosDB.Extensions;

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