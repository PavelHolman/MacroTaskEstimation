namespace MacroTaskEstimation;

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

public interface ICluster<T>
{
    ICluster<T> Merge(ICluster<T> a, ICluster<T> b);
}

public class HierarchicalAgglomerativeClustering<T, Dissimilarity>
    where Dissimilarity : IComparable<Dissimilarity>
{
    private class DissimilarityMatrix
    {
        private readonly Dissimilarity?[][] _dissimilarityMatrix;

        public DissimilarityMatrix(ICollection<ICluster<T>> clusters, Linkage merge)
        {
            var n = clusters.Count;
            _dissimilarityMatrix = new Dissimilarity?[n][];
            foreach (var (i, a) in Enumerable.Range(0, n).Zip(clusters))
            {
                _dissimilarityMatrix[i] = new Dissimilarity [n - (i + 1)];
                foreach (var (j, b) in Enumerable.Range(0, i).Zip(clusters))
                {
                    _dissimilarityMatrix[i][j] = merge(a, b);
                }
            }
        }

        public (int, int)? MinDissimilarityClusters()
        {
            return Enumerable.Range(0, _dissimilarityMatrix.Length)
                .SelectMany(i => Enumerable.Range(0, i), (i, j) => (i, j))
                .MinBy(i => _dissimilarityMatrix[i.i][i.j]);
        }
    }


    public delegate Dissimilarity? Linkage(ICluster<T> a, ICluster<T> b);

    private readonly Linkage _merge;

    public HierarchicalAgglomerativeClustering(Linkage merge)
    {
        _merge = merge;
    }

    public IEnumerable<ICluster<T>> GetClusters(ICollection<ICluster<T>> clusters)
    {
        var n = clusters.Count;
        var dissimilarityMatrix = new DissimilarityMatrix(clusters, _merge);
        for (var i = 0; i < n - 1; i++)
        {
            var indices = dissimilarityMatrix.MinDissimilarityClusters();

            if (!indices.HasValue)
            {
                return clusters.Where((c, i) => find(i) == i);
            }

            var (ai, bi) = indices.Value;
            var a = clusters.ElementAt(ai);
            var b = clusters.ElementAt(bi);

            union(a, b);
        }
    }
}
