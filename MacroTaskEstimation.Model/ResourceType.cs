namespace MacroTaskEstimation.Model;

using MacroTaskEstimation.Model.Interface;

public class ResourceType(IEnumerable<IResourceType.Resource> resources) : IResourceType
{
    public IEnumerable<IResourceType.Resource> Resources { get; } = resources;
}
