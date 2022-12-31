using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using EventFlow.Aggregates;
using EventFlow.Core;
using EventFlow.Core.RetryStrategies;
using EventFlow.Exceptions;
using EventFlow.Extensions;
using EventFlow.ReadStores;
using LisaScheers.EventFlowAddons.CosmosDB.ValueObjects;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace LisaScheers.EventFlowAddons.CosmosDB.ReadStore
{
    public class CosmosDbReadModelStore<TReadModel> : ICosmosDbReadModelStore<TReadModel>
        where TReadModel : class, ICosmosDbReadModel
    {
        private readonly Database _database;
        private readonly ILogger<CosmosDbReadModelStore<TReadModel>> _logger;
        private readonly IReadModelDescriptionProvider _readModelDescriptionProvider;
        private readonly ITransientFaultHandler<IOptimisticConcurrencyRetryStrategy> _transientFaultHandler;

        public CosmosDbReadModelStore(
            ILogger<CosmosDbReadModelStore<TReadModel>> logger,
            Database database,
            IReadModelDescriptionProvider readModelDescriptionProvider,
            ITransientFaultHandler<IOptimisticConcurrencyRetryStrategy> transientFaultHandler)
        {
            _logger = logger;
            _database = database;
            _readModelDescriptionProvider = readModelDescriptionProvider;
            _transientFaultHandler = transientFaultHandler;
        }

        public async Task DeleteAsync(string id, CancellationToken cancellationToken)
        {
            var readModelDescription = _readModelDescriptionProvider.GetReadModelDescription<TReadModel>();
            _logger.LogInformation(
                "Deleting '{ReadModelType}' with id '{Id}', from '{@RootCollectionName}'!",
                typeof(TReadModel).PrettyPrint(),
                id,
                readModelDescription.RootContainerName);
            var container = _database.GetContainer(readModelDescription.RootContainerName.Value);
            await container.DeleteItemAsync<TReadModel>(id, new PartitionKey(id), cancellationToken: cancellationToken);
        }

        public async Task DeleteAllAsync(CancellationToken cancellationToken)
        {
            var readModelDescription = _readModelDescriptionProvider.GetReadModelDescription<TReadModel>();
            _logger.LogInformation(
                "Deleting all '{ReadModelType}' from '{@RootCollectionName}'!",
                typeof(TReadModel).PrettyPrint(),
                readModelDescription.RootContainerName);

            var container = _database.GetContainer(readModelDescription.RootContainerName.Value);
            var query = container.GetItemQueryIterator<TReadModel>(new QueryDefinition("SELECT * FROM c"));
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync(cancellationToken);
                foreach (var item in response)
                    await container.DeleteItemAsync<TReadModel>(item.Id, new PartitionKey(item.Id),
                        cancellationToken: cancellationToken);
            }
        }

        public async Task<ReadModelEnvelope<TReadModel>> GetAsync(string id, CancellationToken cancellationToken)
        {
            var readModelDescription = _readModelDescriptionProvider.GetReadModelDescription<TReadModel>();
            _logger.LogInformation(
                "Getting '{ReadModelType}' with id '{Id}', from '{@RootCollectionName}'!",
                typeof(TReadModel).PrettyPrint(),
                id,
                readModelDescription.RootContainerName);

            var container = _database.GetContainer(readModelDescription.RootContainerName.Value);
            try
            {
                var response =
                    await container.ReadItemAsync<TReadModel>(id, new PartitionKey(id),
                        cancellationToken: cancellationToken);
                return ReadModelEnvelope<TReadModel>.With(id, response.Resource, response.Resource.Version);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return ReadModelEnvelope<TReadModel>.Empty(id);
            }
        }

        public async Task UpdateAsync(IReadOnlyCollection<ReadModelUpdate> readModelUpdates,
            IReadModelContextFactory readModelContextFactory,
            Func<IReadModelContext, IReadOnlyCollection<IDomainEvent>, ReadModelEnvelope<TReadModel>, CancellationToken,
                Task<ReadModelUpdateResult<TReadModel>>> updateReadModel, CancellationToken cancellationToken)
        {
            var readModelDescription = _readModelDescriptionProvider.GetReadModelDescription<TReadModel>();
            _logger.LogInformation(
                "Updating '{ReadModelType}' with id '{Id}', from '{@RootCollectionName}'!",
                typeof(TReadModel).PrettyPrint(),
                readModelUpdates.Select(x => x.ReadModelId).Aggregate((x, y) => $"{x}, {y}"),
                readModelDescription.RootContainerName);
            foreach (var readModelUpdate in readModelUpdates)
                await _transientFaultHandler.TryAsync(
                    c => UpdateReadModelAsync(readModelDescription, readModelUpdate, readModelContextFactory,
                        updateReadModel, c),
                    Label.Named("cosmosdb-readmodel-update")
                    , cancellationToken
                ).ConfigureAwait(false);
        }

        public IOrderedQueryable<TReadModel> GetItemLinqQueryable()
        {
            var readModelDescription = _readModelDescriptionProvider.GetReadModelDescription<TReadModel>();

            var container = _database.GetContainer(readModelDescription.RootContainerName.Value);

            return container.GetItemLinqQueryable<TReadModel>(true);
        }

        public async Task<TReadModel> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            var readModelDescription = _readModelDescriptionProvider.GetReadModelDescription<TReadModel>();
            _logger.LogInformation(
                "Getting '{ReadModelType}' with id '{Id}', from '{@RootCollectionName}'!",
                typeof(TReadModel).PrettyPrint(),
                id,
                readModelDescription.RootContainerName);

            var container = _database.GetContainer(readModelDescription.RootContainerName.Value);
            var response =
                await container.ReadItemAsync<TReadModel>(id, new PartitionKey(id),
                    cancellationToken: cancellationToken);
            return response.Resource;
        }

        public async Task InitDatabaseAsync(CancellationToken cancellationToken = default)
        {
            await _database.CreateContainerIfNotExistsAsync(
                _readModelDescriptionProvider.GetReadModelDescription<TReadModel>().RootContainerName.Value,
                "/id", cancellationToken: cancellationToken);
        }

        private async Task UpdateReadModelAsync(ReadModelDescription readModelDescription,
            ReadModelUpdate readModelUpdate,
            IReadModelContextFactory readModelContextFactory,
            Func<IReadModelContext, IReadOnlyCollection<IDomainEvent>, ReadModelEnvelope<TReadModel>, CancellationToken,
                Task<ReadModelUpdateResult<TReadModel>>> updateReadModel,
            CancellationToken cancellationToken)
        {
            var container = _database.GetContainer(readModelDescription.RootContainerName.Value);
            // get the read model
            ItemResponse<TReadModel>? response;
            try
            {
                response = await container.ReadItemAsync<TReadModel>(readModelUpdate.ReadModelId,
                    new PartitionKey(readModelUpdate.ReadModelId), cancellationToken: cancellationToken);
            }
            catch (Exception e) when (e is CosmosException {StatusCode: HttpStatusCode.NotFound})
            {
                response = null;
            }


            var isAvailable = response != null;
            var readModelEnvelope = isAvailable
                ? ReadModelEnvelope<TReadModel>.With(readModelUpdate.ReadModelId, response!.Resource)
                : ReadModelEnvelope<TReadModel>.Empty(readModelUpdate.ReadModelId);
            var readModelContext = readModelContextFactory.Create(readModelUpdate.ReadModelId, !isAvailable);
            var readModelUpdateResult =
                await updateReadModel(readModelContext, readModelUpdate.DomainEvents, readModelEnvelope,
                        cancellationToken)
                    .ConfigureAwait(false);

            if (!readModelUpdateResult.IsModified) return;

            if (readModelContext.IsMarkedForDeletion)
            {
                if (isAvailable)
                    await container.DeleteItemAsync<TReadModel>(readModelUpdate.ReadModelId,
                        new PartitionKey(readModelUpdate.ReadModelId), cancellationToken: cancellationToken);
                return;
            }

            readModelEnvelope = readModelUpdateResult.Envelope;
            readModelEnvelope.ReadModel.Version = readModelEnvelope.Version;
            try
            {
                if (isAvailable)
                    await container.ReplaceItemAsync(readModelEnvelope.ReadModel, readModelUpdate.ReadModelId,
                        new PartitionKey(readModelUpdate.ReadModelId), new ItemRequestOptions
                        {
                            IfMatchEtag = response?.ETag
                        }, cancellationToken);
                else
                    await container.CreateItemAsync(readModelEnvelope.ReadModel,
                        new PartitionKey(readModelUpdate.ReadModelId),
                        cancellationToken: cancellationToken);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.PreconditionFailed)
            {
                throw new OptimisticConcurrencyException(
                    $"Read model with ID '{readModelUpdate.ReadModelId}' was updated by another process", ex
                );
            }
        }
    }
}