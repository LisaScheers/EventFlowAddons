using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Elastic.Transport.Products.Elasticsearch;
using EventFlow.Aggregates;
using EventFlow.Core;
using EventFlow.Core.RetryStrategies;
using EventFlow.Exceptions;
using EventFlow.Extensions;
using EventFlow.ReadStores;
using Microsoft.Extensions.Logging;

namespace LisaScheers.EventFlowAddons.Elastic.ReadStore
{
    public class ElasticSearchReadModelStore<TReadModel> : IElasticSearchReadModelStore<TReadModel>
        where TReadModel : class, IElasticSearchReadModel
    {
        private readonly ElasticsearchClient _elasticClient;
        private readonly ILogger<ElasticSearchReadModelStore<TReadModel>> _logger;
        private readonly ITransientFaultHandler<IOptimisticConcurrencyRetryStrategy> _transientFaultHandler;
        private readonly IndexName _indexName;
        public ElasticSearchReadModelStore(
            ElasticsearchClient elasticClient,
            ILogger<ElasticSearchReadModelStore<TReadModel>> logger,
            ITransientFaultHandler<IOptimisticConcurrencyRetryStrategy> transientFaultHandler
        )
        {
            _elasticClient = elasticClient;
            _logger = logger;
            _transientFaultHandler = transientFaultHandler;
            _indexName = Indices.Index<TReadModel>();
        }


        public async Task<SearchResponse<TReadModel>> GetWithQueryAsync(
            Action<SearchRequestDescriptor<TReadModel>> query,
            CancellationToken cancellationToken = default
        )
        {
            return await _elasticClient.SearchAsync(query, cancellationToken);
        }
        
        public async Task<SearchResponse<TReadModel>> GetWithQueryAsync(
            SearchRequest<TReadModel> query,
            CancellationToken cancellationToken = default
        )
        {
            return await _elasticClient.SearchAsync<TReadModel>(query, cancellationToken);
        }

        public async Task DeleteAsync(string id, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Deleting read model {ReadModelId}", id);
            var resp = await _elasticClient.DeleteAsync<TReadModel>(id, cancellationToken);
            // log warnings
            LogWarnings(resp);
            LogErrors(resp);
        }

        public async Task DeleteAllAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("Deleting all read models");

            var resp = await _elasticClient.DeleteByQueryAsync<TReadModel>(_indexName,
                descriptor => descriptor.Query(new MatchAllQuery()), cancellationToken);
            LogWarnings(resp);
            LogErrors(resp);
        }

        public async Task<ReadModelEnvelope<TReadModel>> GetAsync(string id, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Getting read model {ReadModelId}", id);

            var resp = await _elasticClient.GetAsync<TReadModel>(id, cancellationToken);
            LogWarnings(resp);
            LogErrors(resp);
            return resp.IsValidResponse
                ? ReadModelEnvelope<TReadModel>.With(id, resp.Source, resp.Version)
                : ReadModelEnvelope<TReadModel>.Empty(id);
        }

        public async Task UpdateAsync(IReadOnlyCollection<ReadModelUpdate> readModelUpdates,
            IReadModelContextFactory readModelContextFactory,
            Func<IReadModelContext, IReadOnlyCollection<IDomainEvent>, ReadModelEnvelope<TReadModel>, CancellationToken,
                Task<ReadModelUpdateResult<TReadModel>>> updateReadModel, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Updating '{ReadModelType}' with id '{Id}'!",
                typeof(TReadModel).PrettyPrint(),
                readModelUpdates.Select(x => x.ReadModelId).Aggregate((x, y) => $"{x}, {y}"));

            foreach (var readModelUpdate in readModelUpdates)
                await _transientFaultHandler.TryAsync(
                    c => UpdateReadModelAsync(readModelUpdate, readModelContextFactory, updateReadModel,
                        cancellationToken),
                    Label.Named("elastic-readmodel-update"), cancellationToken);
            
            // force index refresh
            _logger.LogDebug("Refreshing index");
            LogWarnings(await _elasticClient.Indices.RefreshAsync(_indexName, cancellationToken));
        }


        private async Task UpdateReadModelAsync(
            ReadModelUpdate readModelUpdate,
            IReadModelContextFactory readModelContextFactory,
            Func<IReadModelContext, IReadOnlyCollection<IDomainEvent>, ReadModelEnvelope<TReadModel>, CancellationToken,
                Task<ReadModelUpdateResult<TReadModel>>> updateReadModel,
            CancellationToken cancellationToken)
        {
            _logger.LogDebug("Updating read model {ReadModelId}", readModelUpdate.ReadModelId);

            // get the read model

            var resp = await _elasticClient.GetAsync<TReadModel>(readModelUpdate.ReadModelId, cancellationToken);
            LogWarnings(resp);
            var readModelEnvelope = resp.IsValidResponse
                ? ReadModelEnvelope<TReadModel>.With(readModelUpdate.ReadModelId, resp.Source, resp.Version)
                : ReadModelEnvelope<TReadModel>.Empty(readModelUpdate.ReadModelId);

            var isAvailable = resp.IsValidResponse;
            if (!isAvailable)
                _logger.LogDebug("Read model {ReadModelId} is not available", readModelUpdate.ReadModelId);

            // update the read model
            var readModelContext = readModelContextFactory.Create(readModelUpdate.ReadModelId, !isAvailable);
            var updateResult = await updateReadModel(readModelContext, readModelUpdate.DomainEvents,
                readModelEnvelope, cancellationToken).ConfigureAwait(false);

            // check if modified
            if (!updateResult.IsModified)
            {
                _logger.LogDebug("Read model {ReadModelId} is not modified", readModelUpdate.ReadModelId);
                return;
            }

            // check if marked for deletion
            if (readModelContext.IsMarkedForDeletion)
            {
                if (!isAvailable) return;
                _logger.LogDebug("Deleting read model {ReadModelId}", readModelUpdate.ReadModelId);
                var deleteResp =
                    await _elasticClient.DeleteAsync<TReadModel>(readModelUpdate.ReadModelId, cancellationToken);
                LogWarnings(deleteResp);
                LogErrors(deleteResp);
                return;
            }


            readModelEnvelope = updateResult.Envelope;
            readModelEnvelope.ReadModel.Version = readModelEnvelope.Version;

            if (!isAvailable)
            {
                // create the read model
                _logger.LogDebug("Creating read model {ReadModelId}", readModelUpdate.ReadModelId);
                var createResp = await _elasticClient.IndexAsync<TReadModel>(readModelEnvelope.ReadModel, cancellationToken);
                LogWarnings(createResp);
                if (!createResp.IsValidResponse && createResp.TryGetOriginalException(out var ex)) throw ex;
                return;
            }

            // update the read model and check for concurrency
            _logger.LogDebug("Updating read model {ReadModelId}", readModelUpdate.ReadModelId);
            var updateResp = await _elasticClient.UpdateAsync<TReadModel, TReadModel>(_indexName, readModelEnvelope.ReadModelId,
                descriptor => descriptor.Doc(readModelEnvelope.ReadModel).IfPrimaryTerm(resp.PrimaryTerm).IfSeqNo(resp.SeqNo),
                cancellationToken);
            LogWarnings(updateResp);
            if (!updateResp.IsValidResponse && updateResp.TryGetOriginalException(out var updateEx))
            {
                _logger.LogError( updateEx,"Read model {ReadModelId} is out of sync", readModelUpdate.ReadModelId);
                throw new OptimisticConcurrencyException("Read model update failed due to concurrency violation",
                    updateEx);
            }
        }

        private void LogWarnings(ElasticsearchResponse resp)
        {
            foreach (var warning in resp.ElasticsearchWarnings)
                _logger.LogWarning("Elasticsearch warning: {Warning}", warning);
        }

        private bool LogErrors(ElasticsearchResponse resp)
        {
            if (resp.IsValidResponse) return false;
            if (!resp.TryGetOriginalException(out var ex)) return true;
            _logger.LogError(ex, "Elasticsearch error");
            return true;
        }
    }
}