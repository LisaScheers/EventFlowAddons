namespace EventFlowAddons.CosmosDB.EventStore;

public interface ICosmosDbEventPersistenceInitializer
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}