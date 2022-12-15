using EventFlow.Snapshots;
using EventFlow.ValueObjects;
using Newtonsoft.Json;

namespace EventFlowAddons.CosmosDB.ValueObjects;

public class CosmosDbSnapshotDataModel: ValueObject
{
    [JsonProperty(propertyName:"id")]
    public string Id { get; set; } = null!;

    [JsonProperty(propertyName:"aggregateId")]
    public string AggregateId { get; set; } = null!;

    [JsonProperty(propertyName:"aggregateName")]
    public string AggregateName { get; set; } = null!;

    [JsonProperty(propertyName:"aggregateSequenceNumber")]
    public int AggregateSequenceNumber { get; set; }
    [JsonProperty(propertyName:"data")]
    public string Data { get; set; } = null!;

    [JsonProperty(propertyName:"metaData")]
    public string Metadata { get; set; } = null!;

  
}