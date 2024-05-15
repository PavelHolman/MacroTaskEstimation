namespace MacroTaskEstimation.OwnImpl;

using System.Diagnostics.CodeAnalysis;

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
        var clusters = new HashSet<Cluster>(data); // TODO: index
        while (_condition(clusters) && GetNearestClusters(clusters, out var a, out var b))
        {
            clusters.Remove(a);
            clusters.Remove(b);
            clusters.Add(_merge(a, b));
        }

        return clusters;
    }

    private bool GetNearestClusters(
        IReadOnlyCollection<Cluster> clusters,
        [NotNullWhen(true)] out Cluster? x,
        [NotNullWhen(true)] out Cluster? y)
    {
        Dissimilarity? minDissimilarity = null;
        x = y = default;

        foreach (var (a, b) in clusters
                     .SelectMany((_, i) => clusters.Take(i), (a, b) => (a, b)))
        {
            var dissimilarity = _linkage(a, b);
            if (dissimilarity == null ||
                (minDissimilarity.HasValue && dissimilarity.Value.CompareTo(minDissimilarity.Value) >= 0)) continue;

            (minDissimilarity, x, y) = (dissimilarity, a, b);
        }

        return minDissimilarity.HasValue;
    }
}
