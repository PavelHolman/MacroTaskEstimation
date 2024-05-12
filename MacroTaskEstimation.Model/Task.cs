namespace MacroTaskEstimation.Model;

using MacroTaskEstimation.Model.Interface;

public class Task(int id, TimeSpan duration, DateTime dueDate, IEnumerable<IResourceType> resourceTypes)
    : IComparable<Task>
{
    public int Id { get; } = id;

    public TimeSpan Duration { get; } = duration;

    public DateTime DueDate { get; } = dueDate;

    public IEnumerable<IResourceType> ResourceTypes { get; } = resourceTypes;
    public int CompareTo(Task? other) => this.DueDate.CompareTo(other?.DueDate);
}
