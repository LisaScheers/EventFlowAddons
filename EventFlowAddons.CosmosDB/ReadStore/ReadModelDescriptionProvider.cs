using System.Collections.Concurrent;
using System.Reflection;
using EventFlow.Extensions;
using EventFlowAddons.CosmosDB.ReadStore.Attributes;
using EventFlowAddons.CosmosDB.ValueObjects;

namespace EventFlowAddons.CosmosDB.ReadStore;

public class ReadModelDescriptionProvider : IReadModelDescriptionProvider
{
    private static readonly ConcurrentDictionary<Type, ReadModelDescription> CollectionNames = new();

    public ReadModelDescription GetReadModelDescription<TReadModel>() where TReadModel : ICosmosDbReadModel
    {
        return CollectionNames.GetOrAdd(
            typeof(TReadModel),
            t =>
            {
                var collectionType = t.GetTypeInfo().GetCustomAttribute<CosmosDbContainerNameAttribute>();
                var indexName = collectionType == null
                    ? $"eventflow-{typeof(TReadModel).PrettyPrint().ToLowerInvariant()}"
                    : collectionType.ContainerName;
                return new ReadModelDescription(new RootContainerName(indexName));
            });
    }
}