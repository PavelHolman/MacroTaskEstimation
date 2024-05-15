namespace MacroTaskEstimation.Model;

using MacroTaskEstimation.Model.Interface;

public class ResourceType(int id) : IResourceType
{
    public int Id { get; } = id;
}
