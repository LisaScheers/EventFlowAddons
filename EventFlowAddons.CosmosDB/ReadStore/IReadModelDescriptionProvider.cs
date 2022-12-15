using LisaScheers.EventFlowAddons.CosmosDB.ValueObjects;

namespace LisaScheers.EventFlowAddons.CosmosDB.ReadStore;

public interface IReadModelDescriptionProvider
{
    ReadModelDescription GetReadModelDescription<TReadModel>()
        where TReadModel : ICosmosDbReadModel;
}

