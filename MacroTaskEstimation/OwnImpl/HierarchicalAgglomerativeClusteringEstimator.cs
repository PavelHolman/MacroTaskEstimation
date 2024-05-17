namespace MacroTaskEstimation.OwnImpl;

public class HierarchicalAgglomerativeClustering<T, Cluster, Dissimilarity>
    where Cluster : IEnumerable<T>
    where Dissimilarity : struct, IComparable<Dissimilarity>
{
    public delegate bool Condition(IReadOnlyCollection<Cluster> clusters);

    public delegate Dissimilarity? Linkage(Cluster a, Cluster b);

    public delegate Cluster Merge(Cluster a, Cluster b);

    private readonly Condition _condition;
    private readonly Linkage _linkage;
    private readonly Merge _merge;

    public HierarchicalAgglomerativeClustering(Condition condition, Linkage linkage, Merge merge)
    {
        _condition = condition;
        _linkage = linkage;
        _merge = merge;
    }

    public IEnumerable<Cluster> MergeClusters(IEnumerable<Cluster> data)
    {
        var clusters = new HashSet<Cluster>(data);

        var clusterQueue = new PriorityQueue<(Cluster, Cluster), Dissimilarity>(clusters
            .SelectMany((_, i) => clusters.Take(i), (a, b) => (NearestClusters: (a, b), Dissimilarity: _linkage(a, b)))
            .Where(t => t.Dissimilarity.HasValue)
            .Select(t => (t.NearestClusters, t.Dissimilarity!.Value)));

        while (_condition(clusters) && clusterQueue.TryDequeue(out var nearestClusters, out _))
        {
            var (a, b) = nearestClusters;
            if (!clusters.Contains(a) || !clusters.Contains(b)) continue;
            clusters.Remove(a);
            clusters.Remove(b);
            var merged = _merge(a, b);
            clusterQueue.EnqueueRange(clusters
                .Select(c => (NearestClusters: (c, merged), Dissimilarity: _linkage(c, merged)))
                .Where(t => t.Dissimilarity.HasValue)
                .Select(t => (t.NearestClusters, t.Dissimilarity!.Value)));
            clusters.Add(merged);
        }

        return clusters;
    }
}
