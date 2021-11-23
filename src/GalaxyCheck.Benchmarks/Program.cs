using BenchmarkDotNet.Running;

namespace GalaxyCheck.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<RngBenchmark>();
        }
    }
}
