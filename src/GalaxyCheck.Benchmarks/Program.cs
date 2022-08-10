using BenchmarkDotNet.Running;

namespace GalaxyCheck.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<RngBenchmark>();
            BenchmarkRunner.Run<ReflectedGenBenchmark>();
            BenchmarkRunner.Run<ListGenBenchmark>();
            BenchmarkRunner.Run<SelectManyGenBenchmark>();
            BenchmarkRunner.Run<SelectGenBenchmark>();
        }
    }
}
