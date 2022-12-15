using EventFlow.Aggregates;
using EventFlow.ReadStores;
using EventFlow.TestHelpers.Aggregates;
using EventFlow.TestHelpers.Aggregates.Events;
using LisaScheers.EventFlowAddons.CosmosDB.ReadStore;
using LisaScheers.EventFlowAddons.CosmosDB.ReadStore.Attributes;

namespace EventFlow.CosmosDB.Tests.IntegrationTests.ReadStores.ReadModels;

[CosmosDbContainerName("thingy")]
public class CosmosDbThingyReadModel : ICosmosDbReadModel,
    IAmReadModelFor<ThingyAggregate, ThingyId, ThingyDomainErrorAfterFirstEvent>,
    IAmReadModelFor<ThingyAggregate, ThingyId, ThingyPingEvent>,
    IAmReadModelFor<ThingyAggregate, ThingyId, ThingyDeletedEvent>
{
    public bool DomainErrorAfterFirstReceived { get; set; }

    public int PingsReceived { get; set; }

    public Task ApplyAsync(IReadModelContext context,
        IDomainEvent<ThingyAggregate, ThingyId, ThingyDeletedEvent> domainEvent,
        CancellationToken cancellationToken)
    {
        context.MarkForDeletion();

        return Task.CompletedTask;
    }

    public Task ApplyAsync(IReadModelContext context,
        IDomainEvent<ThingyAggregate, ThingyId, ThingyDomainErrorAfterFirstEvent> domainEvent,
        CancellationToken cancellationToken)
    {
        Id = domainEvent.AggregateIdentity.Value;
        DomainErrorAfterFirstReceived = true;

        return Task.CompletedTask;
    }

    public Task ApplyAsync(IReadModelContext context,
        IDomainEvent<ThingyAggregate, ThingyId, ThingyPingEvent> domainEvent,
        CancellationToken cancellationToken)
    {
        Id = domainEvent.AggregateIdentity.Value;
        PingsReceived++;

        return Task.CompletedTask;
    }

    public string Id { get; set; }
    public long? Version { get; set; }

    public Thingy ToThingy()
    {
        return new Thingy(
            ThingyId.With(Id),
            PingsReceived,
            DomainErrorAfterFirstReceived);
    }
}