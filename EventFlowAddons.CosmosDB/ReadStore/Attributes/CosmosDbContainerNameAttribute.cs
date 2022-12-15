namespace LisaScheers.EventFlowAddons.CosmosDB.ReadStore.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class CosmosDbContainerNameAttribute : Attribute
{
    public CosmosDbContainerNameAttribute(string containerName)
    {
        ContainerName = containerName;
    }

    public string ContainerName { get; }
}