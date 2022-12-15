namespace EventFlowAddons.CosmosDB.EventStore;

public interface ICosmosDbEventSequenceStore
{
    /* 
   * This method is used to get the next sequence number for a given aggregate and update the counter in the database.
   * @param name The aggregate id for which the next sequence number is required.
   * @return The next sequence number for the given aggregate.
   * @throws CosmosException if the database operation fails.
   * 
   */
    Task<ulong> GetLastSequenceNumberAsync(CancellationToken cancellationToken = default);
}