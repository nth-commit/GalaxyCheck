using BenchmarkDotNet.Attributes;
using GalaxyCheck.Gens.Parameters.Internal;

namespace GalaxyCheck.Benchmarks
{
    [SimpleJob]
    public class RngBenchmark
    {
        private const int OperationsPerInvoke = 100_000;
        private int _seed = 0;

        [IterationCleanup]
        public void IncrementRng()
        {
            _seed += OperationsPerInvoke;
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        [Arguments(0, 10)]
        [Arguments(0, 100)]
        [Arguments(0, 1000)]
        [Arguments(int.MinValue, int.MaxValue)]
        public void RandomIntInRange(int MinValue, int MaxValue)
        {
            for (var seed = _seed; seed < (_seed + OperationsPerInvoke); seed++)
            {
                var rng = Rng.Create(seed);
                rng.Value(MinValue, MaxValue);
            }
        }
    }
}
