namespace MacroTaskEstimation.Model;

using MacroTaskEstimation.Model.Interface;

public class Task(DateTime dueDate, IEnumerable<IResourceType> resourceTypes)
{
    public DateTime DueDate { get; } = dueDate;

    public IEnumerable<IResourceType> ResourceTypes { get; } = resourceTypes;
}
