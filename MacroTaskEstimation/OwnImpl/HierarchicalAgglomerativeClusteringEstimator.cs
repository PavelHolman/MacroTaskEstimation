namespace MacroTaskEstimation.OwnImpl;

using System.Diagnostics.CodeAnalysis;

/* Input:
   - Data points X
   - Merge Clusters function (a, b) -> Opt<dissimilarity>
   - Optionally allows the merge, can limit the max distance itself

   Output:
   - Clusters C

   1. Initialize each data point as its own cluster:
      C = [C(x) for x in X]

   2. While the number of clusters is greater than 1:
       a. Find the two closest clusters according to the distance metric:
          (cluster1, cluster2) = argmin(dist(cluster1, cluster2) for all cluster1, cluster2 in C)

       b. Check if merging the two clusters satisfies the criterion:
          if criterion(cluster1, cluster2) <= Limit:
              Merge cluster1 and cluster2 into a new cluster:
              new_cluster = cluster1 ∪ cluster2
              C.remove(cluster1)
              C.remove(cluster2)
              C.append(new_cluster)
          else:
              Break the loop

   3. Return the final clusters C
 */

public class HierarchicalAgglomerativeClustering<Cluster, Dissimilarity>
    where Dissimilarity : IComparable<Dissimilarity>
{
    public delegate Dissimilarity? Linkage(Cluster a, Cluster b);

    public delegate Cluster Merge(Cluster a, Cluster b);

    private readonly Linkage _linkage;
    private readonly Merge _merge;

    public HierarchicalAgglomerativeClustering(Linkage linkage, Merge merge)
    {
        _linkage = linkage;
        _merge = merge;
    }

    public List<Cluster> MergeClusters(IEnumerable<Cluster> data)
    {
        var clusters = new HashSet<Cluster>(data); // TODO: index
        while (GetNearestClusters(clusters, out var a, out var b))
        {
            clusters.Remove(a);
            clusters.Remove(b);
            clusters.Add(_merge(a, b));
        }

        return clusters.ToList();
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
