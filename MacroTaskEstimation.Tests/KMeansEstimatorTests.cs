// namespace MacroTaskEstimation.Tests;
//
// using MacroTaskEstimation.Model;
// using MacroTaskEstimation.Model.Interface;
//
// public class KMeansEstimatorTests
// {
//     [Test]
//     public void Test1()
//     {
//         var estimator = new KMeansEstimator(seed: 42);
//
//         var dueDateEarly = DateTime.Today;
//         var dueDateLater = dueDateEarly.AddYears(3);
//
//         var resource = new IResourceType.Resource([(1, 2), (3, 4)]);
//         var resourceType = new ResourceType([resource]);
//
//         var task1 = new Task(dueDateEarly, new HashSet<IResourceType>([resourceType]));
//         var task2 = new Task(dueDateEarly, new HashSet<IResourceType>([resourceType]));
//         var task3 = new Task(dueDateEarly, new HashSet<IResourceType>([resourceType]));
//
//         var task4 = new Task(dueDateLater, new HashSet<IResourceType>([resourceType]));
//         var task5 = new Task(dueDateLater, new HashSet<IResourceType>([resourceType]));
//
//         var macroTasks = estimator.EstimateMacroTasks([task1, task2, task3, task4, task5]);
//
//         Assert.That(macroTasks, Is.EquivalentTo(new List<List<Task>>([[task1, task2, task3], [task4, task5]])));
//     }
// }
