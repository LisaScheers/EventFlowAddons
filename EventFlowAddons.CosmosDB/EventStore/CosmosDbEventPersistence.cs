using EventFlow.Aggregates;
using EventFlow.Core;
using EventFlow.EventStores;
using EventFlow.Exceptions;
using LisaScheers.EventFlowAddons.CosmosDB.ValueObjects;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace LisaScheers.EventFlowAddons.CosmosDB.EventStore;

public class CosmosDbEventPersistence : IEventPersistence
{
    private readonly ICosmosDbEventSequenceStore _cosmosDbEventSequenceStore;
    private readonly Database _database;
    private readonly Container _eventContainer;
    private readonly ILogger<CosmosDbEventPersistence> _logger;

    public CosmosDbEventPersistence(ILogger<CosmosDbEventPersistence> logger, Database database,
        ICosmosDbEventSequenceStore cosmosDbEventSequenceStore)
    {
        _logger = logger;
        _database = database;
        _cosmosDbEventSequenceStore = cosmosDbEventSequenceStore;
        _eventContainer = _database.GetContainer("events");
    }


    public async Task<AllCommittedEventsPage> LoadAllCommittedEvents(GlobalPosition globalPosition, int pageSize,
        CancellationToken cancellationToken)
    {
        var position = ulong.Parse(string.IsNullOrWhiteSpace(globalPosition.Value) ? "0" : globalPosition.Value);
        var events = _eventContainer.GetItemLinqQueryable<CosmosDbEventDataModel>(true)
            .Where(e => e.GlobalSequenceNumber > position)
            .OrderBy(e => e.GlobalSequenceNumber)
            .Take(pageSize).ToAsyncEnumerable();

        var allCommittedEvents = await events.Select(@event => @event).ToListAsync(cancellationToken);

        var nextPosition = allCommittedEvents.Count == 0 ? position : allCommittedEvents.Last().GlobalSequenceNumber;

        var page = new AllCommittedEventsPage(new GlobalPosition(nextPosition.ToString()), allCommittedEvents);
        return page;
    }

    public async Task<IReadOnlyCollection<ICommittedDomainEvent>> CommitEventsAsync(IIdentity id,
        IReadOnlyCollection<SerializedEvent> serializedEvents, CancellationToken cancellationToken)
    {
        if (!serializedEvents.Any()) return new List<ICommittedDomainEvent>();
        var committedEvents = new List<ICommittedDomainEvent>();

        foreach (var @event in serializedEvents)
        {
            var globalSequenceNumber =
                await _cosmosDbEventSequenceStore.GetLastSequenceNumberAsync(cancellationToken);
            var committedEvent = new CosmosDbEventDataModel
            {
                id = globalSequenceNumber.ToString(),
                AggregateId = id.Value,
                GlobalSequenceNumber = globalSequenceNumber,
                AggregateName = @event.Metadata[MetadataKeys.AggregateName],
                Data = @event.SerializedData,
                Metadata = @event.SerializedMetadata,
                AggregateSequenceNumber = @event.AggregateSequenceNumber,
                BatchId = @event.Metadata[MetadataKeys.BatchId]
            };
            committedEvents.Add(committedEvent);
        }

        var batch = _eventContainer.CreateTransactionalBatch(new PartitionKey(id.Value));
        foreach (var committedEvent in committedEvents) batch.CreateItem(committedEvent);

        var response = await batch.ExecuteAsync(cancellationToken);
        if (response.IsSuccessStatusCode) return committedEvents;

        _logger.LogError("Failed to commit events to CosmosDB. Response status code: {StatusCode}",
            response.StatusCode);
        throw new OptimisticConcurrencyException("Failed to commit events to CosmosDB");
    }

    public async Task<IReadOnlyCollection<ICommittedDomainEvent>> LoadCommittedEventsAsync(IIdentity id,
        int fromEventSequenceNumber, CancellationToken cancellationToken)
    {
        var events = _eventContainer.GetItemLinqQueryable<CosmosDbEventDataModel>(true)
            .Where(e => e.AggregateId == id.Value && e.AggregateSequenceNumber >= fromEventSequenceNumber)
            .OrderBy(e => e.AggregateSequenceNumber)
            .ToAsyncEnumerable();

        var committedEvents = await events.Select(@event => @event).ToListAsync(cancellationToken);
        return committedEvents;
    }

    public async Task DeleteEventsAsync(IIdentity id, CancellationToken cancellationToken)
    {
        var events = _eventContainer.GetItemLinqQueryable<CosmosDbEventDataModel>(true)
            .Where(e => e.AggregateId == id.Value)
            .ToAsyncEnumerable();

        var committedEvents = await events.Select(@event => @event).ToListAsync(cancellationToken);
        foreach (var @event in committedEvents)
            await _eventContainer.DeleteItemAsync<CosmosDbEventDataModel>(@event.id,
                new PartitionKey(@event.AggregateId), cancellationToken: cancellationToken);
    }
}