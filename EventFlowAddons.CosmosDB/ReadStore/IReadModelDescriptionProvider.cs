using EventFlowAddons.CosmosDB.ValueObjects;

namespace EventFlowAddons.CosmosDB.ReadStore;

public interface IReadModelDescriptionProvider
{
    ReadModelDescription GetReadModelDescription<TReadModel>()
        where TReadModel : ICosmosDbReadModel;
}

