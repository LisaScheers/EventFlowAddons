using EventFlow.Aggregates;
using EventFlow.ReadStores;
using EventFlow.TestHelpers.Aggregates;
using EventFlow.TestHelpers.Aggregates.Entities;
using EventFlow.TestHelpers.Aggregates.Events;
using LisaScheers.EventFlowAddons.CosmosDB.ReadStore;
using LisaScheers.EventFlowAddons.CosmosDB.ReadStore.Attributes;

namespace EventFlow.CosmosDB.Tests.IntegrationTests.ReadStores.ReadModels;

[CosmosDbContainerName("thingymessage")]
public class CosmosDbThingyMessageReadModel : ICosmosDbReadModel,
    IAmReadModelFor<ThingyAggregate, ThingyId, ThingyMessageAddedEvent>,
    IAmReadModelFor<ThingyAggregate, ThingyId, ThingyMessageHistoryAddedEvent>
{
    public string ThingyId { get; set; }

    public string Message { get; set; }

    public Task ApplyAsync(IReadModelContext context,
        IDomainEvent<ThingyAggregate, ThingyId, ThingyMessageAddedEvent> domainEvent,
        CancellationToken cancellationToken)
    {
        ThingyId = domainEvent.AggregateIdentity.Value;

        var thingyMessage = domainEvent.AggregateEvent.ThingyMessage;
        Id = thingyMessage.Id.Value;
        Message = thingyMessage.Message;

        return Task.CompletedTask;
    }

    public Task ApplyAsync(IReadModelContext context,
        IDomainEvent<ThingyAggregate, ThingyId, ThingyMessageHistoryAddedEvent> domainEvent,
        CancellationToken cancellationToken)
    {
        ThingyId = domainEvent.AggregateIdentity.Value;

        var messageId = new ThingyMessageId(context.ReadModelId);
        var thingyMessage = domainEvent.AggregateEvent.ThingyMessages.Single(m => m.Id == messageId);
        Id = messageId.Value;
        Message = thingyMessage.Message;

        return Task.CompletedTask;
    }

    public string Id { get; set; }
    public long? Version { get; set; }

    public ThingyMessage ToThingyMessage()
    {
        return new ThingyMessage(
            ThingyMessageId.With(Id),
            Message);
    }
}