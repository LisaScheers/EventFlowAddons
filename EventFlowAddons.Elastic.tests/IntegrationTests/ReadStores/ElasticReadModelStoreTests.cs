using System;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using EventFlow;
using EventFlow.Extensions;
using EventFlow.TestHelpers;
using EventFlow.TestHelpers.Aggregates.Entities;
using EventFlow.TestHelpers.Suites;
using LisaScheers.EventFlowAddons.Elastic.Extensions;
using LisaScheers.EventFlowAddons.Elastic.Tests.IntegrationTests.ReadStores.QueryHandlers;
using LisaScheers.EventFlowAddons.Elastic.Tests.IntegrationTests.ReadStores.ReadModels;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace LisaScheers.EventFlowAddons.Elastic.Tests.IntegrationTests.ReadStores
{



    [Category(Categories.Integration)]
    [TestFixture]
    public class ElasticReadModelStoreTests : TestSuiteForReadModelStore
    {
        [TearDown]
        public void TearDown()
        {
            _client.Indices.Delete(Indices.Index<ElasticThingyReadModel>());
            _client.Indices.Delete(Indices.Index<ElasticThingyMessageReadModel>());
        }

        protected override Type ReadModelType { get; } = typeof(ElasticThingyReadModel);

        private ElasticsearchClient _client;

        protected override IServiceProvider Configure(IEventFlowOptions eventFlowOptions)
        {
            init();
            var resolver = eventFlowOptions
                .RegisterServices(sr => sr.AddTransient(typeof(ThingyMessageLocator)))
                .ConfigureElastic(() => _client)
                .UseElasticReadModel<ElasticThingyReadModel>()
                .UseElasticReadModel<ElasticThingyMessageReadModel, ThingyMessageLocator>()
                .AddQueryHandlers(
                    typeof(ElasticThingyGetQueryHandler),
                    typeof(ElasticThingyGetVersionQueryHandler),
                    typeof(ElasticThingyGetMessagesQueryHandler)
                );

            return base.Configure(resolver);
        }
        
        private void CreateIndex()
        {
            _client.Indices.Create<ElasticThingyReadModel>(c => c
                .Index("thingies")
                .Mappings(m =>
                    m.Properties(p =>
                        p.Text(o => o.Id)
                            .LongNumber(o => o.Version)
                            .Boolean(o => o.DomainErrorAfterFirstReceived)
                            .IntegerNumber(o => o.PingsReceived)
                    )
                )
            );

            _client.Indices.Create<ElasticThingyMessageReadModel>(
                c =>
                    c.Index("thingy-messages")
                        .Mappings(m =>
                            m.Properties(p =>
                                p.Text(o => o.Id)
                                    .LongNumber(o => o.Version)
                                    .Text(o => o.Message)
                                    .Text(o => o.ThingyId)
                            )
                        )
            ); 
        }
        
        private void init()
        {
            
            _client = new ElasticsearchClient(new ElasticsearchClientSettings(new Uri("http://localhost:9200"))
                .DefaultMappingFor<ElasticThingyMessageReadModel>(m => m
                    .IndexName("thingy-messages")
                    .IdProperty(t => t.Id))
                .DefaultMappingFor<ElasticThingyReadModel>(m => m
                    .IndexName("thingies")
                    .IdProperty(t => t.Id))
            );
                    
            // map the read model to the index
            
            CreateIndex();
        }
    }
}