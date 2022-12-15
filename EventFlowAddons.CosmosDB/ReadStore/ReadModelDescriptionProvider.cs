using System;
using System.Collections.Concurrent;
using System.Reflection;
using EventFlow.Extensions;
using LisaScheers.EventFlowAddons.CosmosDB.ReadStore.Attributes;
using LisaScheers.EventFlowAddons.CosmosDB.ValueObjects;

namespace LisaScheers.EventFlowAddons.CosmosDB.ReadStore
{

    public class ReadModelDescriptionProvider : IReadModelDescriptionProvider
    {
        private static readonly ConcurrentDictionary<Type, ReadModelDescription> CollectionNames = new ConcurrentDictionary<Type, ReadModelDescription>();

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
}