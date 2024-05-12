namespace MacroTaskEstimation.Model;

using MacroTaskEstimation.Model.Interface;

public class ResourceType(int id, IEnumerable<IResourceType.Resource> resources) : IResourceType
{
    public int Id { get; } = id;

    public IEnumerable<IResourceType.Resource> Resources { get; } = resources;
}
