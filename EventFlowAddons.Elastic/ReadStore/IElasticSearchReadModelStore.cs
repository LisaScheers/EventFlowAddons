using System;
using System.Threading;
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;
using EventFlow.ReadStores;

namespace LisaScheers.EventFlowAddons.Elastic.ReadStore
{
    public interface IElasticSearchReadModelStore<TReadModel>:  IReadModelStore<TReadModel>
        where TReadModel : class, IReadModel
    {
        public Task<SearchResponse<TReadModel>> GetWithQueryAsync(
            Action<SearchRequestDescriptor<TReadModel>> query,
            CancellationToken cancellationToken = default
        );
        
        public Task<SearchResponse<TReadModel>> GetWithQueryAsync(
            SearchRequest<TReadModel> query,
            CancellationToken cancellationToken = default
        );
    }
}