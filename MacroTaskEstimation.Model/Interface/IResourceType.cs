namespace MacroTaskEstimation.Model.Interface;

public interface IResourceType
{
    public int Capacity => Resources.Count();

    public IEnumerable<IResource> Resources { get; init; }

    public interface IResource
    {
        public IEnumerable<(int From, int To)> Calendars { get; init; }
    }
}
