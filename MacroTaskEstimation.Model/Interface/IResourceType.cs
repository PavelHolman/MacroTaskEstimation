namespace MacroTaskEstimation.Model.Interface;

public interface IResourceType
{
    public IEnumerable<Resource> Resources { get; }

    public class Resource(IEnumerable<(int From, int To)> calendars)
    {
        public IEnumerable<(int From, int To)> Calendars { get; } = calendars;
    }
}
