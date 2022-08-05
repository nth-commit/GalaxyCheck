using BenchmarkDotNet.Attributes;

namespace GalaxyCheck.Benchmarks
{
    [SimpleJob]
    public class ReflectedGenBenchmark
    {
        private class ClassWith10IntMembers
        {
            public int Property0 { get; set; }
            public int Property1 { get; set; }
            public int Property2 { get; set; }
            public int Property3 { get; set; }
            public int Property4 { get; set; }
            public int Property5 { get; set; }
            public int Property6 { get; set; }
            public int Property7 { get; set; }
            public int Property8 { get; set; }
            public int Property9 { get; set; }
        }

        private int _seed = 0;

        [IterationCleanup]
        public void IncrementRng()
        {
            _seed += 1;
        }

        [Benchmark(OperationsPerInvoke = 1)]
        [Arguments(1)]
        [Arguments(10)]
        [Arguments(100)]
        public void SampleClassWith10IntMembers(int iterations)
        {
            var gen = Gen.Create<ClassWith10IntMembers>();

            gen.Sample(iterations: iterations, seed: _seed);
        }

        private class ClassWith10StringMembers
        {
            public string Property0 { get; set; }
            public string Property1 { get; set; }
            public string Property2 { get; set; }
            public string Property3 { get; set; }
            public string Property4 { get; set; }
            public string Property5 { get; set; }
            public string Property6 { get; set; }
            public string Property7 { get; set; }
            public string Property8 { get; set; }
            public string Property9 { get; set; }
        }

        [Benchmark(OperationsPerInvoke = 1)]
        [Arguments(1)]
        [Arguments(10)]
        [Arguments(100)]
        public void SampleClassWith10StringMembers(int iterations)
        {
            var gen = Gen.Create<ClassWith10StringMembers>();

            gen.Sample(iterations: iterations, seed: _seed);
        }
    }
}
