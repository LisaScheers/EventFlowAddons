using EventFlow.ReadStores;
using Newtonsoft.Json;

namespace LisaScheers.EventFlowAddons.CosmosDB.ReadStore
{

    public interface ICosmosDbReadModel : IReadModel
    {
        [JsonProperty("id")] public string Id { get; set; }

        [JsonProperty("version")] public long? Version { get; set; }
    }
}