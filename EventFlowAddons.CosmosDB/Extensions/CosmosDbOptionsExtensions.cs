using EventFlow;
using EventFlow.Extensions;
using EventFlow.ReadStores;
using EventFlowAddons.CosmosDB.EventStore;
using EventFlowAddons.CosmosDB.ReadStore;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;


namespace EventFlowAddons.CosmosDB.Extensions;

public static class CosmosDbOptionsExtensions
{
    public static IEventFlowOptions ConfigureCosmosDb(this IEventFlowOptions eventFlowOptions,
        Func<Database> databaseFactory)
    {
        var database = databaseFactory();
        eventFlowOptions.ServiceCollection.TryAddSingleton<Database>(database);
        eventFlowOptions.ServiceCollection.TryAddSingleton<IReadModelDescriptionProvider, ReadModelDescriptionProvider>();
        eventFlowOptions.ServiceCollection.TryAddSingleton<ICosmosDbEventSequenceStore, CosmosDbEventSequenceStore>();

        return eventFlowOptions;
    }
    public static IEventFlowOptions UseCosmosDbReadModel<TReadModel>(
        this IEventFlowOptions eventFlowOptions)
        where TReadModel : class, ICosmosDbReadModel
    {
        eventFlowOptions.ServiceCollection.TryAddTransient<ICosmosDbReadModelStore<TReadModel>, CosmosDbReadModelStore<TReadModel>>();
        eventFlowOptions.ServiceCollection.TryAddTransient<IReadModelStore<TReadModel>>(r => r.GetService<ICosmosDbReadModelStore<TReadModel>>() ?? throw new InvalidOperationException());
            
        eventFlowOptions.UseReadStoreFor<ICosmosDbReadModelStore<TReadModel>, TReadModel>();
            
        return eventFlowOptions;
    }
    
    public static IEventFlowOptions UseCosmosDbReadModel<TReadModel, TReadModelLocator>(
        this IEventFlowOptions eventFlowOptions)
        where TReadModel : class, ICosmosDbReadModel
        where TReadModelLocator : IReadModelLocator
    {
        eventFlowOptions.ServiceCollection.TryAddTransient<ICosmosDbReadModelStore<TReadModel>, CosmosDbReadModelStore<TReadModel>>();
        eventFlowOptions.ServiceCollection.TryAddTransient<IReadModelStore<TReadModel>>(r => r.GetService<ICosmosDbReadModelStore<TReadModel>>() ?? throw new InvalidOperationException());
            
        eventFlowOptions.UseReadStoreFor<ICosmosDbReadModelStore<TReadModel>, TReadModel, TReadModelLocator>();
            
        return eventFlowOptions;
    }
    

}