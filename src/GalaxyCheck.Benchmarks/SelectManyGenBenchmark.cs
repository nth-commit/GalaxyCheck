using BenchmarkDotNet.Attributes;
using GalaxyCheck.Gens.Parameters;
using System.Linq;

namespace GalaxyCheck.Benchmarks
{
    [SimpleJob]
    public class SelectManyGenBenchmark
    {

        private const int OperationsPerInvoke = 10000;

        private int _seed = 0;

        [IterationCleanup]
        public void IncrementRng()
        {
            _seed += 1;
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void SelectManyGen()
        {
            var gen =
                from x in Gen.Int32()
                from y in Gen.Int32()
                select (x, y);

            var enumerable = gen.Advanced
                .Run(GenParameters.Parse(seed: _seed, size: 100))
                .Take(OperationsPerInvoke);

            foreach (var _ in enumerable) { }
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void SelectManyGenBaseline()
        {
            var gen = Gen.Int32();

            var enumerable = gen.Advanced
                .Run(GenParameters.Parse(seed: _seed, size: 100))
                .Take(OperationsPerInvoke * 2);

            foreach (var _ in enumerable) { }
        }
    }
}
