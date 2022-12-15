using EventFlow.ReadStores;

namespace LisaScheers.EventFlowAddons.CosmosDB.ReadStore;

public interface ICosmosDbReadModelStore<TReadModel> : IReadModelStore<TReadModel> where TReadModel : class, IReadModel

{
    public IOrderedQueryable<TReadModel> GetItemLinqQueryable();
    public Task<TReadModel> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    
    public Task InitDatabaseAsync(CancellationToken cancellationToken = default);
}