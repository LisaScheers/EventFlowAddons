using Microsoft.Azure.Cosmos;

namespace LisaScheers.EventFlowAddons.CosmosDB.EventStore;

public class CosmosDbEventPersistenceInitializer : ICosmosDbEventPersistenceInitializer
{
    private readonly Database _database;

    public CosmosDbEventPersistenceInitializer(Database database)
    {
        _database = database;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        // create event counter container 
        var eventContainer = await _database.CreateContainerIfNotExistsAsync(
            new ContainerProperties("counter", "/id"),
            cancellationToken: cancellationToken);
        // create event store container
        var eventStoreContainer = await _database.CreateContainerIfNotExistsAsync(
            new ContainerProperties("events", "/AggregateId")
            {
                UniqueKeyPolicy = new UniqueKeyPolicy
                {
                    UniqueKeys =
                    {
                        new UniqueKey
                        {
                            Paths = {"/AggregateId", "/AggregateSequenceNumber"}
                        }
                    }
                }
            },
            cancellationToken: cancellationToken);
    }
}