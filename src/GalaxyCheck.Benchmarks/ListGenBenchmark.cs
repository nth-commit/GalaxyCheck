using BenchmarkDotNet.Attributes;
using GalaxyCheck.Gens.Parameters;
using System.Linq;

namespace GalaxyCheck.Benchmarks
{
    [SimpleJob]
    public class ListGenBenchmark
    {

        private const int OperationsPerInvoke = 100;

        private int _seed = 0;

        [IterationCleanup]
        public void IncrementRng()
        {
            _seed += 1;
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        [Arguments(10)]
        [Arguments(50)]
        [Arguments(100)]
        [Arguments(500)]
        public void ListGen(int size)
        {
            var gen = Gen.Int32().ListOf().WithCount(size);

            var enumerable = gen.Advanced
                .Run(GenParameters.Create(_seed, 100))
                .Take(OperationsPerInvoke);

            foreach (var _ in enumerable) { }
        }
    }
}
