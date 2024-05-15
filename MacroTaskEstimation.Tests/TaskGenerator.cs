namespace MacroTaskEstimation.Tests;

using MacroTaskEstimation.Model;
using MacroTaskEstimation.Model.Interface;

public static class TaskGenerator
{
    private static Random rng;

    public static List<Task> GenerateTasks(
        int taskCount, int resourceTypeCount, int maxResourceTypesOnTask, int maxTaskDuration, int seed)
    {
        rng = new Random(seed);
        var resourceTypes = GenerateResourceTypes(resourceTypeCount);
        return GenerateTasks(taskCount, resourceTypes, maxResourceTypesOnTask, maxTaskDuration);
    }

    private static List<Task> GenerateTasks(
        int taskCount, IList<ResourceType> resourceTypes, int maxResourceTypesOnTask, int maxTaskDuration)
    {
        var startDate = DateTime.Today;
        var endDate = startDate.AddYears(1);
        var range = endDate - startDate;

        return Enumerable.Range(0, taskCount)
            .Select(id => new Task(
                id,
                TimeSpan.FromHours(rng.Next(1, maxTaskDuration)),
                startDate + TimeSpan.FromDays(rng.NextInt64(0, (int)range.TotalDays)),
                resourceTypes.TakeRandom(rng.Next(1, maxResourceTypesOnTask))))
            .ToList();
    }

    private static IEnumerable<T> TakeRandom<T>(this IList<T> source, int count)
    {
        var indices = new HashSet<int>();

        while (indices.Count < count)
        {
            indices.Add(rng.Next(source.Count));
        }

        foreach (var index in indices)
        {
            yield return source[index];
        }
    }

    private static IList<ResourceType> GenerateResourceTypes(int resourceTypeCount)
    {
        return Enumerable.Range(0, resourceTypeCount)
            .Select(id => new ResourceType(id))
            .ToList();
    }
}
