namespace EventFlowAddons.CosmosDB.ReadStore.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class CosmosDbContainerNameAttribute: Attribute
{
    public string ContainerName { get; }

    public CosmosDbContainerNameAttribute(string containerName)
    {
        ContainerName = containerName;
    }   
}