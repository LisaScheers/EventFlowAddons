using System.Threading;
using System.Threading.Tasks;

namespace LisaScheers.EventFlowAddons.CosmosDB.EventStore
{

    public interface ICosmosDbEventPersistenceInitializer
    {
        Task InitializeAsync(CancellationToken cancellationToken = default);
    }
}