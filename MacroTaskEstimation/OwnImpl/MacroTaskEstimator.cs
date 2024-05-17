namespace MacroTaskEstimation.OwnImpl;

using MacroTaskEstimation.Model;

public class MacroTaskEstimatorConfig
{
    private static readonly MacroTaskLinkage AverageLinkage = (a, b) => a
        .SelectMany(x => b
            .Select(y => double.Abs((x.DueDate - y.DueDate).TotalMinutes)))
        .Sum() / (a.Count * b.Count);

    private static readonly MacroTaskLinkage DefaultLinkage = AverageLinkage;

    private static readonly MacroTaskMerge DefaultMerge = (a, b) => a.Concat(b).ToList();

    public delegate double MacroTaskLinkage(List<Task> a, List<Task> b);

    public delegate List<Task> MacroTaskMerge(List<Task> a, List<Task> b);

    public int MinimumClusters { get; init; } = 1;
    public TimeSpan MaxDueDateDifference { get; init; } = TimeSpan.MaxValue;
    public TimeSpan MaxDurationSum { get; init; } = TimeSpan.MaxValue;
    public MacroTaskLinkage Linkage { get; init; } = DefaultLinkage;
    public MacroTaskMerge Merge { get; init; } = DefaultMerge;
}

public class MacroTaskEstimator
{
    private readonly HierarchicalAgglomerativeClustering<Task, List<Task>, double> _clustering;

    public MacroTaskEstimator(MacroTaskEstimatorConfig config)
    {
        _clustering = new HierarchicalAgglomerativeClustering<Task, List<Task>, double>(
            macroTasks => macroTasks.Count > config.MinimumClusters,
            (a, b) => CanMergeTasks(a, b) ? config.Linkage(a, b) : null,
            (a, b) => config.Merge(a, b));

        bool CanMergeTasks(IReadOnlyCollection<Task> tasks1, IReadOnlyCollection<Task> tasks2)
        {
            var totalDuration = tasks1
                .Concat(tasks2)
                .Select(t => t.Duration)
                .Aggregate((duration1, duration2) => duration1 + duration2);

            var minDueDate = tasks1
                .Concat(tasks2)
                .Select(t => t.DueDate)
                .Min();

            var maxDueDate = tasks1
                .Concat(tasks2)
                .Select(t => t.DueDate)
                .Max();

            return totalDuration <= config.MaxDurationSum && maxDueDate - minDueDate <= config.MaxDueDateDifference;
        }
    }

    public IEnumerable<List<Task>> EstimateMacroTasks(IEnumerable<Task> tasks)
    {
        return tasks
            .SelectMany(task => task.ResourceTypes,
                (task, rt) => (Task: task, ResourceType: rt))
            .GroupBy(p => p.ResourceType, p => p.Task)
            .AsParallel()
            .SelectMany(taskGroup => _clustering.MergeClusters(taskGroup.Select(t => new List<Task> { t })));
    }
}
