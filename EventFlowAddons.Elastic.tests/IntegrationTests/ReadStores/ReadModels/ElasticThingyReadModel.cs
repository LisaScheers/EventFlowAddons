using System.Threading;
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;
using EventFlow.Aggregates;
using EventFlow.ReadStores;
using EventFlow.TestHelpers.Aggregates;
using EventFlow.TestHelpers.Aggregates.Events;
using LisaScheers.EventFlowAddons.Elastic.ReadStore;

namespace LisaScheers.EventFlowAddons.Elastic.Tests.IntegrationTests.ReadStores.ReadModels
{



    public class ElasticThingyReadModel : IElasticSearchReadModel,
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

        public Id Id { get; set; }
        public long? Version { get; set; }

        public Thingy ToThingy()
        {
            return new Thingy(
                ThingyId.With(Id.ToString()),
                PingsReceived,
                DomainErrorAfterFirstReceived);
        }
    }
}