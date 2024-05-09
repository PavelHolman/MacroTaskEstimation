namespace MacroTaskEstimation;

using System.Collections.Frozen;
using Aglomera;
using Aglomera.Linkage;
using MacroTaskEstimation.Model;

public class AgglomerativeClusteringEstimator(TimeSpan maxDuration)
{
    public List<List<Task>> EstimateMacroTasks(IEnumerable<Task> tasks)
    {
        return tasks
            .SelectMany(task => task.ResourceTypes,
                (task, rt) => (Task: task, ResourceType: rt))
            .GroupBy(p => p.ResourceType, p => p.Task)
            .Select(Enumerable.ToList)
            .SelectMany(this.Estimate)
            .ToList();
    }

    private List<List<Task>> Estimate(ICollection<Task> tasks)
    {
        var dataPoints = tasks.Select(task => new DataPoint(task)).ToFrozenSet();

        var metric = new TaskDissimilarityMetric();

        var linkage = new MacroTaskLinkage(new MinimumEnergyLinkage<DataPoint>(metric), CanMergeClusters);

        var algorithm = new AgglomerativeClusteringAlgorithm<DataPoint>(linkage);

        var clustering = algorithm.GetClustering(dataPoints);

        return clustering
            .Last(cluster => !double.IsNaN(cluster.Dissimilarity))
            .Select(cluster => cluster.Select(d => d.Task).ToList())
            .ToList();

        bool CanMergeClusters(Cluster<DataPoint> cluster1, Cluster<DataPoint> cluster2) =>
            cluster1
                .Concat(cluster2)
                .Select(d => d.Task.Duration)
                .Aggregate((duration1, duration2) => duration1 + duration2) <= maxDuration;
    }

    private sealed class DataPoint(Task task) : IComparable<DataPoint>
    {
        public Task Task { get; } = task;

        public int CompareTo(DataPoint? other) => Task.DueDate.CompareTo(other?.Task.DueDate);
    }

    private sealed class TaskDissimilarityMetric : IDissimilarityMetric<DataPoint>
    {
        public double Calculate(DataPoint instance1, DataPoint instance2) =>
            double.Abs((instance1.Task.DueDate - instance2.Task.DueDate).TotalSeconds);
    }

    private sealed class MacroTaskLinkage(
        ILinkageCriterion<DataPoint> linkage,
        MacroTaskLinkage.AreClustersMergeable areClustersMergeable)
        : ILinkageCriterion<DataPoint>
    {
        public delegate bool AreClustersMergeable(Cluster<DataPoint> cluster1, Cluster<DataPoint> cluster2);

        public double Calculate(Cluster<DataPoint> cluster1, Cluster<DataPoint> cluster2)
            => areClustersMergeable(cluster1, cluster2)
                ? linkage.Calculate(cluster1, cluster2)
                : double.NaN;

        public IDissimilarityMetric<DataPoint> DissimilarityMetric { get; } = linkage.DissimilarityMetric;
    }
}
