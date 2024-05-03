namespace MacroTaskEstimation.Model;

using MacroTaskEstimation.Model.Interface;

public class ResourceType(IEnumerable<IResourceType.IResource> resources) : IResourceType
{
    public IEnumerable<IResourceType.IResource> Resources { get; init; } = resources;
}
