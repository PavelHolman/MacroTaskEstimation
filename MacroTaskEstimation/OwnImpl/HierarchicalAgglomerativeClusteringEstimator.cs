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
        [NotNullWhen(true)] out Cluster? a,
        [NotNullWhen(true)] out Cluster? b)
    {
        var clusterDissimilarities = clusters
            .SelectMany((_, i) => clusters.Take(i), (a, b) => (A: a, B: b))
            .Select(mergedClusters => (
                Clusters: mergedClusters,
                Dissimilarity: _linkage(mergedClusters.A, mergedClusters.B)))
            .Where(x => x.Dissimilarity != null)
            .ToList();

        if (clusterDissimilarities.Count == 0)
        {
            a = default;
            b = default;
            return false;
        }

        (a, b) = clusterDissimilarities
            .MinBy(x => x.Dissimilarity)
            .Clusters;

        return true;
    }
}
