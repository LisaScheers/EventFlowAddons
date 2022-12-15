using EventFlow.ValueObjects;

namespace EventFlowAddons.CosmosDB.ValueObjects;

public class ReadModelDescription: ValueObject
{
    public ReadModelDescription(RootContainerName rootContainerName)
    {
        if (rootContainerName == null) throw new ArgumentNullException(nameof(rootContainerName));

        RootContainerName = rootContainerName;
    }

    public RootContainerName RootContainerName { get; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return RootContainerName;
    }
}