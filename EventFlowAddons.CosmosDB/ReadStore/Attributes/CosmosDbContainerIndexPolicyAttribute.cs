using Microsoft.Azure.Cosmos;

namespace EventFlowAddons.CosmosDB.ReadStore.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class CosmosDbContainerIndexPolicyAttribute: Attribute
{
    public IndexingPolicy IndexingPolicy { get; }
    
    public CosmosDbContainerIndexPolicyAttribute(IndexingPolicy indexingPolicy)
    {
        IndexingPolicy = indexingPolicy;
    }

    
}