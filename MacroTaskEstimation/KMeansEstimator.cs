namespace MacroTaskEstimation;

using MacroTaskEstimation.Model;
using Microsoft.ML;
using Microsoft.ML.Data;

public class KMeansEstimator(int? seed = null)
{
    private readonly MLContext _mlContext = new(seed);

    public IEnumerable<List<Task>> EstimateMacroTasks(IEnumerable<Task> tasks)
    {
        return tasks
            .SelectMany(task => task.ResourceTypes,
                (task, rt) => (Task: task, ResourceType: rt))
            .GroupBy(p => p.ResourceType, p => p.Task)
            .Select(Enumerable.ToList)
            .SelectMany(this.Estimate)
            .ToList();
    }

    private List<List<Task>> Estimate(IList<Task> tasks)
    {
        var data = _mlContext.Data.LoadFromEnumerable(tasks.Select(task => new DataPoint(task)));

        var pipeline = _mlContext.Clustering.Trainers.KMeans(numberOfClusters: 2);

        var model = pipeline.Fit(data);
        var transformedData = model.Transform(data);

        var cluster = transformedData.GetColumn<uint>("PredictedLabel");

        return cluster.Zip(tasks, (label, task) => (Label: label, Task: task))
            .GroupBy(z => z.Label, z => z.Task)
            .Select(Enumerable.ToList)
            .ToList();
    }

    private sealed class DataPoint(Task task)
    {
        [VectorType(1)] public float[] Features { get; } = [task.DueDate.Ticks];
    }
}
