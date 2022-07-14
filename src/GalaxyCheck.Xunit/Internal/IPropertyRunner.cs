using System.Threading.Tasks;
using Xunit.Abstractions;

namespace GalaxyCheck.Xunit.Internal
{
    internal record PropertyRunParameters(
        IGen<TestAsync<object>> Property,
        int Iterations,
        int ShrinkLimit,
        string? Replay,
        int? Seed,
        int? Size);

    internal interface IPropertyRunner
    {
        Task Run(PropertyRunParameters parameters, ITestOutputHelper testOutputHelper);
    }
}
