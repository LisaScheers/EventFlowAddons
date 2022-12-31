using System;
using Elastic.Clients.Elasticsearch;
using EventFlow;
using EventFlow.Extensions;
using EventFlow.ReadStores;
using LisaScheers.EventFlowAddons.Elastic.ReadStore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LisaScheers.EventFlowAddons.Elastic.Extensions
{

    public static class ElasticOptionsExtensions
    {
        public static IEventFlowOptions ConfigureElastic(this IEventFlowOptions eventFlowOptions,
            Func<ElasticsearchClient> databaseFactory)
        {
            var database = databaseFactory();
            eventFlowOptions.ServiceCollection.TryAddSingleton(database);
           

            return eventFlowOptions;
        }

        public static IEventFlowOptions UseElasticReadModel<TReadModel>(
            this IEventFlowOptions eventFlowOptions)
            where TReadModel : class, IElasticSearchReadModel
        {
            eventFlowOptions.ServiceCollection
                .TryAddTransient<IElasticSearchReadModelStore<TReadModel>, ElasticSearchReadModelStore<TReadModel>>();
            eventFlowOptions.ServiceCollection.TryAddTransient<IReadModelStore<TReadModel>>(r =>
                r.GetService<IElasticSearchReadModelStore<TReadModel>>() ?? throw new InvalidOperationException());

            eventFlowOptions.UseReadStoreFor<IElasticSearchReadModelStore<TReadModel>, TReadModel>();

            return eventFlowOptions;
        }

        public static IEventFlowOptions UseElasticReadModel< TReadModel, TReadModelLocator>(
            this IEventFlowOptions eventFlowOptions)
            where TReadModel : class, IElasticSearchReadModel
            where TReadModelLocator : IReadModelLocator
        {
            eventFlowOptions.ServiceCollection
                .TryAddTransient<IElasticSearchReadModelStore<TReadModel>, ElasticSearchReadModelStore<TReadModel>>();
            eventFlowOptions.ServiceCollection.TryAddTransient<IReadModelStore<TReadModel>>(r =>
                r.GetService<IElasticSearchReadModelStore<TReadModel>>() ?? throw new InvalidOperationException());

            eventFlowOptions.UseReadStoreFor<IElasticSearchReadModelStore<TReadModel>, TReadModel, TReadModelLocator>();

            return eventFlowOptions;
        }
    }
}