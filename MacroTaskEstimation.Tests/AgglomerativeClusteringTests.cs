namespace MacroTaskEstimation.Tests;

using MacroTaskEstimation.AglomeraImpl;
using MacroTaskEstimation.Model;
using MacroTaskEstimation.Model.Interface;
using MacroTaskEstimation.OwnImpl;

public class AgglomerativeClusteringTests
{
    [Test]
    public void TwoClustersTest()
    {
        var dueDateEarly = DateTime.Today;
        var dueDateLater = dueDateEarly.AddYears(3);

        var resourceType1 = new ResourceType(0);
        var resourceType2 = new ResourceType(1);
        var resourceType3 = new ResourceType(2);

        var task1 = new Task(1, TimeSpan.FromHours(1), dueDateEarly, new HashSet<IResourceType>([resourceType1]));
        var task2 = new Task(2, TimeSpan.FromHours(1), dueDateEarly, new HashSet<IResourceType>([resourceType1]));
        var task3 = new Task(3, TimeSpan.FromHours(1), dueDateEarly, new HashSet<IResourceType>([resourceType1]));

        var task4 = new Task(4, TimeSpan.FromHours(1), dueDateLater, new HashSet<IResourceType>([resourceType1]));
        var task5 = new Task(5, TimeSpan.FromHours(1), dueDateLater, new HashSet<IResourceType>([resourceType1]));

        var task6 = new Task(6, TimeSpan.FromHours(1), dueDateLater, new HashSet<IResourceType>([resourceType2]));
        var task7 = new Task(7, TimeSpan.FromHours(1), dueDateLater, new HashSet<IResourceType>([resourceType3]));

        var clustering = new HierarchicalAgglomerativeClustering<Task, List<Task>, double>(
            _ => true,
            (a, b) =>
            {
                if (a.Concat(b).Select(t => t.Duration).Aggregate((x, y) => x + y) > TimeSpan.FromHours(3))
                {
                    return null;
                }

                return a
                    .SelectMany(x => b
                        .Select(y => double.Abs((x.DueDate - y.DueDate).TotalMinutes)))
                    .Sum() / (a.Count * b.Count);
            },
            (a, b) => [..a, ..b]);

        var macroTasks = new List<Task>([task1, task2, task3, task4, task5, task6, task7])
            .SelectMany(task => task.ResourceTypes,
                (task, rt) => (Task: task, ResourceType: rt))
            .GroupBy(p => p.ResourceType, p => p.Task)
            .Select(Enumerable.ToList)
            .SelectMany(tasks => clustering.MergeClusters(tasks.Select(t => new List<Task> { t })))
            .ToList();

        var expectedMacroTasks = new List<List<Task>>
        {
            new List<Task> { task1, task2, task3 },
            new List<Task> { task4, task5 },
            new List<Task> { task6 },
            new List<Task> { task7 }
        };

        Console.WriteLine(
            string.Join(", ", macroTasks.Select(macroTask => $"[{string.Join(", ", macroTask.Select(t => t.Id))}]")));

        Assert.That(macroTasks.Select(l => l.Select(t => t.Id).Order().ToList()),
            Is.EquivalentTo(expectedMacroTasks.Select(l => l.Select(t => t.Id).Order().ToList()))
                .Using<List<Task>>((expected, actual) =>
                    expected.Count == actual.Count && !expected.Except(actual).Any()));
    }

    [TestCase(10000, 3000, 10, 1, 10)]
    [TestCase(10, 3, 3, 3, 10)]
    [TestCase(250000, 3000, 10, 1, 10)]
    [TestCase(250000, 10000, 100, 10, 10)]
    public void GeneratedDataTest(
        int taskCount, int resourceTypeCount, int maxResourceCount, int maxResourceTypesOnTask, int maxTaskDuration)
    {
        var tasks = TaskGenerator.GenerateTasks(
            taskCount, resourceTypeCount, maxResourceTypesOnTask, maxTaskDuration, seed: 42);

        var start = DateTime.Now;

        var macroTasks1 =
            new AgglomerativeClusteringEstimator(TimeSpan.FromHours(maxTaskDuration)).EstimateMacroTasks(tasks);

        Console.WriteLine(DateTime.Now - start);

        var clustering = new HierarchicalAgglomerativeClustering<Task, List<Task>, double>(
            _ => true,
            (a, b) =>
            {
                if (a.Concat(b)
                        .Select(t => t.Duration)
                        .Aggregate((x, y) => x + y) > TimeSpan.FromHours(maxTaskDuration))
                {
                    return null;
                }

                return a
                    .SelectMany(x => b
                        .Select(y => double.Abs((x.DueDate - y.DueDate).TotalMinutes)))
                    .Sum() / (a.Count * b.Count);
            },
            (a, b) => [..a, ..b]);

        start = DateTime.Now;

        var macroTasks2 =
            tasks
                .SelectMany(task => task.ResourceTypes,
                    (task, rt) => (Task: task, ResourceType: rt))
                .GroupBy(p => p.ResourceType, p => p.Task)
                .Select(Enumerable.ToList)
                .SelectMany(tasks => clustering.MergeClusters(tasks.Select(t => new List<Task> { t })))
                .ToList();

        Console.WriteLine(DateTime.Now - start);

        Assert.That(macroTasks1.Select(l => l.Select(t => t.Id).Order().ToList()),
            Is.EquivalentTo(macroTasks2.Select(l => l.Select(t => t.Id).Order().ToList()))
                .Using<List<Task>>((expected, actual) =>
                    expected.Count == actual.Count && !expected.Except(actual).Any()));
    }
}
