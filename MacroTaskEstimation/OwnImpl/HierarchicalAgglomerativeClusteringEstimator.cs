namespace MacroTaskEstimation.OwnImpl;

using System.Collections;

public abstract class Node<T> : IEnumerable<T>
{
    public abstract IEnumerator<T> GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public class InnerNode<T>(Node<T> left, Node<T> right) : Node<T>
{
    public override IEnumerator<T> GetEnumerator() => left.Concat(right).GetEnumerator();
}

public class LeafNode<T>(T value) : Node<T>
{
    public override IEnumerator<T> GetEnumerator()
    {
        yield return value;
    }
}

public class HierarchicalAgglomerativeClustering<T, Dissimilarity>
    where Dissimilarity : struct, IComparable<Dissimilarity>
{
    public delegate bool Condition(IReadOnlyCollection<IEnumerable<T>> clusters);

    public delegate Dissimilarity? Linkage(IReadOnlyCollection<T> a, IReadOnlyCollection<T> b);

    private readonly Condition _condition;
    private readonly Linkage _linkage;

    public HierarchicalAgglomerativeClustering(Condition condition, Linkage linkage)
    {
        _condition = condition;
        _linkage = linkage;
    }

    public IEnumerable<IEnumerable<T>> MergeClusters(IEnumerable<T> data)
    {
        var clusters = data.Select(c => new LeafNode<T>(c)).ToHashSet<Node<T>>();

        var clusterQueue = new PriorityQueue<(Node<T>, Node<T>), Dissimilarity>(clusters
            .SelectMany((_, i) => clusters.Take(i),
                (Node<T> a, Node<T> b) => (NearestClusters: (a, b), Dissimilarity: _linkage(a.ToList(), b.ToList())))
            .Where(t => t.Dissimilarity.HasValue)
            .Select(t => (t.NearestClusters, t.Dissimilarity!.Value)));

        while (_condition(clusters) && clusterQueue.TryDequeue(out var nearestClusters, out _))
        {
            var (a, b) = nearestClusters;
            clusters.Remove(a);
            clusters.Remove(b);
            var merged = new InnerNode<T>(a, b);
            clusterQueue.EnqueueRange(clusters
                .Select(c => (NearestClusters: (c, (Node<T>)merged),
                    Dissimilarity: _linkage(c.ToList(), merged.ToList())))
                .Where(t => t.Dissimilarity.HasValue)
                .Select(t => (t.NearestClusters, t.Dissimilarity!.Value)));
            clusters.Add(merged);
        }

        return clusters;
    }
}
