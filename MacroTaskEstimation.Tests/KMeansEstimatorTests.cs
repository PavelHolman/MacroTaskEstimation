namespace MacroTaskEstimation.Tests;

using MacroTaskEstimation.Model;
using MacroTaskEstimation.Model.Interface;

public class KMeansEstimatorTests
{
    [Test]
    public void Test1()
    {
        var estimator = new KMeansEstimator(seed: 42);

        var dueDateEarly = DateTime.Today;
        var dueDateLater = dueDateEarly.AddDays(3);

        var rt = new ResourceType([]);

        var task1 = new Task(1, dueDateEarly, new HashSet<IResourceType>([rt]));
        var task2 = new Task(1, dueDateEarly, new HashSet<IResourceType>([rt]));
        var task3 = new Task(1, dueDateEarly, new HashSet<IResourceType>([rt]));

        var task4 = new Task(1, dueDateLater, new HashSet<IResourceType>([rt]));
        var task5 = new Task(1, dueDateLater, new HashSet<IResourceType>([rt]));

        var macroTasks = estimator.EstimateMacroTasks([task1, task2, task3, task4, task5]);

        Assert.That(macroTasks, Is.EquivalentTo(new List<List<Task>>([[task1, task2, task3], [task4, task5]])));
    }
}
