namespace MacroTaskEstimation.Tests;

using Aglomera;
using Aglomera.Linkage;
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

        var oneHour = TimeSpan.FromHours(1);

        var resourceType = new ResourceType(0, [new IResourceType.Resource([(1, 2), (3, 4)])]);

        var task1 = new Task(1, oneHour, dueDateEarly, new HashSet<IResourceType>([resourceType]));
        var task2 = new Task(2, oneHour, dueDateEarly, new HashSet<IResourceType>([resourceType]));
        var task3 = new Task(3, oneHour, dueDateEarly, new HashSet<IResourceType>([resourceType]));

        var task4 = new Task(4, oneHour, dueDateLater, new HashSet<IResourceType>([resourceType]));
        var task5 = new Task(5, oneHour, dueDateLater, new HashSet<IResourceType>([resourceType]));


        var estimator = new AgglomerativeClusteringEstimator(TimeSpan.FromHours(3));
        var macroTasks = estimator.EstimateMacroTasks([task1, task2, task3, task4, task5]);

        var expectedMacroTasks = new List<List<Task>>
        {
            new List<Task> { task1, task2, task3 },
            new List<Task> { task4, task5 }
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
            taskCount, resourceTypeCount, maxResourceCount, maxResourceTypesOnTask, maxTaskDuration);

        var start = DateTime.Now;

        var macroTasks1 =
            new AgglomerativeClusteringEstimator(TimeSpan.FromHours(maxTaskDuration)).EstimateMacroTasks(tasks);

        Console.WriteLine(DateTime.Now - start);

        var maxSpan = TimeSpan.FromHours(maxTaskDuration);

        var clustering = new HierarchicalAgglomerativeClustering<Task, List<Task>, double>(
            (a, b) =>
            {
                var sum = 0.0;
                foreach (var (x, y) in a.SelectMany(x => b.Select(y => (x, y))))
                {
                    sum += double.Abs((x.DueDate - y.DueDate).TotalSeconds);
                }

                return sum > maxSpan.TotalSeconds ? default : sum;
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
