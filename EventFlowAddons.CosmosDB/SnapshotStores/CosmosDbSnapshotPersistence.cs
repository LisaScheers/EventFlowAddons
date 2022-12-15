using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using EventFlow.Core;
using EventFlow.Snapshots;
using EventFlow.Snapshots.Stores;
using LisaScheers.EventFlowAddons.CosmosDB.ValueObjects;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;

namespace LisaScheers.EventFlowAddons.CosmosDB.SnapshotStores
{

    public class CosmosDbSnapshotPersistence : ISnapshotPersistence
    {
        private const string SnapShotsContainerName = "snapShots";
        private readonly Database _database;
        private readonly ILogger<CosmosDbSnapshotPersistence> _logger;

        public CosmosDbSnapshotPersistence(ILogger<CosmosDbSnapshotPersistence> logger, Database database)
        {
            _logger = logger;
            _database = database;
        }

        public async Task<CommittedSnapshot?> GetSnapshotAsync(Type aggregateType, IIdentity identity,
            CancellationToken cancellationToken)
        {
            var container = _database.GetContainer(SnapShotsContainerName);
            _logger.LogInformation("Getting snapshot for aggregate {Name} with id {Value}", aggregateType.Name,
                identity.Value);
            try
            {
                var query = container.GetItemLinqQueryable<CosmosDbSnapshotDataModel>(true)
                    .Where(s => s.AggregateName == aggregateType.Name && s.AggregateId == identity.Value)
                    .OrderByDescending(s => s.AggregateSequenceNumber)
                    .Take(1)
                    .ToFeedIterator();
                var result = await query.ReadNextAsync(cancellationToken);
                var snapshot = result.FirstOrDefault();
                if (snapshot != null) return new CommittedSnapshot(snapshot.Metadata, snapshot.Data);
                _logger.LogInformation("No snapshot found for aggregate {Name} with id {Value}", aggregateType.Name,
                    identity.Value);
                return null;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task SetSnapshotAsync(Type aggregateType, IIdentity identity,
            SerializedSnapshot serializedSnapshot,
            CancellationToken cancellationToken)
        {
            await _database.CreateContainerIfNotExistsAsync(SnapShotsContainerName, "/aggregateId",
                cancellationToken: cancellationToken);

            var container = _database.GetContainer(SnapShotsContainerName);


            _logger.LogInformation("Setting snapshot for aggregate {Name} with id {Value}", aggregateType.Name,
                identity.Value);
            var snapshot = new CosmosDbSnapshotDataModel
            {
                AggregateId = identity.Value,
                AggregateSequenceNumber = serializedSnapshot.Metadata.AggregateSequenceNumber,
                Metadata = serializedSnapshot.SerializedMetadata,
                Data = serializedSnapshot.SerializedData,
                Id = Guid.NewGuid().ToString(),
                AggregateName = aggregateType.Name
            };
            await container.CreateItemAsync(snapshot, cancellationToken: cancellationToken);
        }

        public async Task DeleteSnapshotAsync(Type aggregateType, IIdentity identity,
            CancellationToken cancellationToken)
        {
            var container = _database.GetContainer(SnapShotsContainerName);
            _logger.LogInformation("Deleting snapshot for aggregate {Name} with id {Value}", aggregateType.Name,
                identity.Value);
            try
            {
                await container.DeleteItemAsync<CosmosDbSnapshotDataModel>(identity.Value,
                    new PartitionKey(aggregateType.Name), cancellationToken: cancellationToken);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // ignore
            }
        }

        public async Task PurgeSnapshotsAsync(Type aggregateType, CancellationToken cancellationToken)
        {
            try
            {
                var container = _database.GetContainer(SnapShotsContainerName);
                _logger.LogInformation("Purging snapshots for aggregate {Name}", aggregateType.Name);
                var query = container.GetItemLinqQueryable<CosmosDbSnapshotDataModel>(true)
                    .Where(x => x.AggregateName == aggregateType.Name)
                    .ToFeedIterator();
                while (query.HasMoreResults)
                {
                    var result = query.ReadNextAsync(cancellationToken).Result;
                    foreach (var item in result)
                        try
                        {
                            await container.DeleteItemAsync<CosmosDbSnapshotDataModel>(item.Id,
                                new PartitionKey(item.AggregateName), cancellationToken: cancellationToken);
                        }
                        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                        {
                            // ignore
                        }
                }
            }
            catch (AggregateException ex) when (ex.InnerExceptions.Any(e => e is CosmosException
                                                {
                                                    StatusCode: HttpStatusCode.NotFound
                                                }))
            {
                _logger.LogInformation("No snapshots found for aggregate {Name}", aggregateType.Name);
                // ignore
            }
        }

        public async Task PurgeSnapshotsAsync(CancellationToken cancellationToken)
        {
            try
            {
                var container = _database.GetContainer(SnapShotsContainerName);
                _logger.LogInformation("Purging all snapshots");
                var query = container.GetItemLinqQueryable<CosmosDbSnapshotDataModel>(true)
                    .ToFeedIterator();
                while (query.HasMoreResults)
                {
                    var result = query.ReadNextAsync(cancellationToken).Result;
                    foreach (var item in result)
                        await container.DeleteItemAsync<CosmosDbSnapshotDataModel>(item.Id,
                            new PartitionKey(item.AggregateName), cancellationToken: cancellationToken);
                }
            }
            catch (AggregateException ex) when (ex.InnerExceptions.Any(e => e is CosmosException
                                                {
                                                    StatusCode: HttpStatusCode.NotFound
                                                }))
            {
                _logger.LogInformation("No snapshots found");
                // ignore
            }

        }
    }
}