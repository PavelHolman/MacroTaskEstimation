// namespace MacroTaskEstimation.Tests;
//
// using BenchmarkDotNet.Attributes;
// using MacroTaskEstimation.Model;
//
// public class KMeansBench
// {
//     [Benchmark]
//     [ArgumentsSource(nameof(Data))]
//     public void EstimateGeneratedTasks(List<Task> tasks)
//     {
//         var estimator = new KMeansEstimator(seed: 42);
//         var res = estimator.EstimateMacroTasks(tasks).Select(Enumerable.ToList).ToList();
//
//         Console.WriteLine(res);
//     }
//
//     public IEnumerable<object[]> Data()
//     {
//         yield return [TaskGenerator.GenerateTasks(10000, 10, 10, 10)];
//         yield return [TaskGenerator.GenerateTasks(10000, 1000, 10, 10)];
//         yield return [TaskGenerator.GenerateTasks(250000, 1000, 1000, 1000)];
//     }
// }
