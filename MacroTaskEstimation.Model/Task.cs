namespace MacroTaskEstimation.Model;

using MacroTaskEstimation.Model.Interface;

public class Task(int duration, DateTime dueDate, ISet<IResourceType> resourceTypes)
{
    private static int _idCounter = 0;

    public int Id { get; } = _idCounter++;

    public int Duration { get; } = duration;

    public DateTime DueDate { get; } = dueDate;

    public ISet<IResourceType> ResourceTypes { get; } = resourceTypes;
}
