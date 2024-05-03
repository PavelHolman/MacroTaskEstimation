namespace MacroTaskEstimation.Tests;

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

public class KMeansEstimatorBenchmarks
{
    [Test]
    public void Benchmark()
    {
        BenchmarkRunner.Run<KMeansBench>(new DebugInProcessConfig());

        Assert.Pass();
    }
}
