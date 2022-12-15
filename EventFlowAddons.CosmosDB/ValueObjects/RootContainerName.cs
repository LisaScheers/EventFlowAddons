using EventFlow.ValueObjects;

namespace EventFlowAddons.CosmosDB.ValueObjects;

public class RootContainerName: SingleValueObject<string>
{
    public RootContainerName(string value) : base(value)
    {
        if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(value));
    }
}