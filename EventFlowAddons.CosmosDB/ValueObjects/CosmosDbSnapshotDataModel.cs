using EventFlow.ValueObjects;
using Newtonsoft.Json;

namespace EventFlowAddons.CosmosDB.ValueObjects;

public class CosmosDbSnapshotDataModel : ValueObject
{
    [JsonProperty("id")] public string Id { get; set; } = null!;

    [JsonProperty("aggregateId")] public string AggregateId { get; set; } = null!;

    [JsonProperty("aggregateName")] public string AggregateName { get; set; } = null!;

    [JsonProperty("aggregateSequenceNumber")]
    public int AggregateSequenceNumber { get; set; }

    [JsonProperty("data")] public string Data { get; set; } = null!;

    [JsonProperty("metaData")] public string Metadata { get; set; } = null!;
}