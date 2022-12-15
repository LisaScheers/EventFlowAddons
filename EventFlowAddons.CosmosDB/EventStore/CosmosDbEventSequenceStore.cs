using System.Text.Json.Serialization;
using EventFlowAddons.CosmosDB.ValueObjects;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;

namespace EventFlowAddons.CosmosDB.EventStore;

public class CosmosDbEventSequenceStore: ICosmosDbEventSequenceStore
{
   private const string _collectionName = "counter";
   private readonly Database _database;
   

   public CosmosDbEventSequenceStore(Database CosmosDbDatabase)
   {
      _database = CosmosDbDatabase;
   }

   public async Task<ulong> GetLastSequenceNumberAsync( CancellationToken cancellationToken = default)
   {
      // check is the counter document exists
      var container = _database.GetContainer(_collectionName);
      try
      {
         var response = await container.ReadItemAsync<Counter>("counter", new PartitionKey("counter"), cancellationToken: cancellationToken);
         var counter = response.Resource;
         counter.SequenceNumber++;
         await container.UpsertItemAsync(counter, new PartitionKey("counter"), cancellationToken: cancellationToken);

         return counter.SequenceNumber;
      }
      catch (Exception e)
      {
         var resp = await container.UpsertItemAsync(new Counter(){SequenceNumber = 1}, new PartitionKey("counter"), cancellationToken: cancellationToken);
         var counter = resp.Resource;
         return counter.SequenceNumber;
      }
      
   }

}



public class Counter
{
   public string id { get; set; } = "counter";
   public ulong SequenceNumber { get; set; }
}

