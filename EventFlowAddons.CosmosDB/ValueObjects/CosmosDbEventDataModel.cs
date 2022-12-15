using EventFlow.EventStores;
using EventFlow.ValueObjects;
using Newtonsoft.Json;

namespace EventFlowAddons.CosmosDB.ValueObjects;

public class CosmosDbEventDataModel: ValueObject, ICommittedDomainEvent
{
    public string id { get; set; } = null!;
    public ulong GlobalSequenceNumber { get; set; }
    
    public string BatchId { get; set; }
    
    public string AggregateId { get; set; } = null!;

    public string AggregateName { get; set; } = null!;

    public int AggregateSequenceNumber { get; set; }

    public string Data { get; set; } = null!;

    public string Metadata { get; set; } = null!;
}