namespace MacroTaskEstimation.Tests;

using MacroTaskEstimation.Model;
using MacroTaskEstimation.Model.Interface;

public static class TaskGenerator
{
    public static List<Task> GenerateTasks(
        int taskCount, int resourceTypeCount, int maxResourceCount, int maxResourcesOnTask)
    {
        var resourceTypes = GenerateResourceTypes(resourceTypeCount, maxResourceCount);
        return GenerateTasks(taskCount, resourceTypes, maxResourcesOnTask);
    }

    private static List<Task> GenerateTasks(
        int taskCount, IList<ResourceType> resourceTypes, int maxResourceCount)
    {
        var startDate = DateTime.Today;
        var endDate = startDate.AddYears(1);
        var range = endDate - startDate;

        return Enumerable.Range(0, taskCount)
            .Select(_ => new Task(
                startDate + TimeSpan.FromDays(Random.Shared.NextInt64(0, (int)range.TotalDays)),
                resourceTypes.TakeRandom(Random.Shared.Next(1, maxResourceCount))))
            .ToList();
    }

    private static IEnumerable<IResourceType.Resource> GenerateResources(int count) =>
        Enumerable.Range(0, count).Select(_ => new IResourceType.Resource([]));

    private static IEnumerable<T> TakeRandom<T>(this IList<T> source, int count)
    {
        var indices = new HashSet<int>();

        while (indices.Count < count)
        {
            indices.Add(Random.Shared.Next(source.Count));
        }

        foreach (var index in indices)
        {
            yield return source[index];
        }
    }

    private static IList<ResourceType> GenerateResourceTypes(int resourceTypeCount, int maxResourceCount)
    {
        return Enumerable.Range(0, resourceTypeCount)
            .Select(_ => new ResourceType(GenerateResources(Random.Shared.Next(1, maxResourceCount))))
            .ToList();
    }
}
