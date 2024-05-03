namespace MacroTaskEstimation;

using MacroTaskEstimation.Model;
using Microsoft.ML;
using Microsoft.ML.Data;

public class KMeansEstimator(int? seed = null)
{
    private readonly MLContext _mlContext = new(seed);

    public IEnumerable<IEnumerable<Task>> EstimateMacroTasks(IEnumerable<Task> tasks)
    {
        return tasks
            .SelectMany(task => task.ResourceTypes,
                (task, rt) => (Task: task, ResourceType: rt))
            .GroupBy(p => p.ResourceType, p => p.Task)
            .Select(Enumerable.ToList)
            .SelectMany(this.Estimate);
    }

    private IEnumerable<IEnumerable<Task>> Estimate(ICollection<Task> tasks)
    {
        var data = _mlContext.Data.LoadFromEnumerable(tasks.Select(DataPoint.FromTask));

        return _mlContext.Clustering.Trainers.KMeans(numberOfClusters: 2)
            .Fit(data)
            .Transform(data)
            .GetColumn<uint>("PredictedLabel")
            .Zip(tasks, (label, task) => (Label: label, Task: task))
            .GroupBy(z => z.Label, z => z.Task);
    }

    private sealed class DataPoint(Task task)
    {
        [VectorType(1)] public float[] Features { get; } = [task.DueDate.Ticks];

        public static DataPoint FromTask(Task task) => new(task);
    }
}
