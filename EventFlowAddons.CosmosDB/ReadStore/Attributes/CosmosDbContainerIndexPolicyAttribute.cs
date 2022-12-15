using Microsoft.Azure.Cosmos;

namespace EventFlowAddons.CosmosDB.ReadStore.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class CosmosDbContainerIndexPolicyAttribute : Attribute
{
    public CosmosDbContainerIndexPolicyAttribute(IndexingPolicy indexingPolicy)
    {
        IndexingPolicy = indexingPolicy;
    }

    public IndexingPolicy IndexingPolicy { get; }
}