using Elastic.Clients.Elasticsearch;
using EventFlow.ReadStores;

namespace LisaScheers.EventFlowAddons.Elastic.ReadStore
{
    public interface IElasticSearchReadModel: IReadModel
    {
        public Id Id { get; set; }
        public long? Version { get; set; }
    }
}