using GalaxyCheck.Gens;
using Xunit.Abstractions;

namespace GalaxyCheck.Xunit.Internal
{
    internal record PropertyRunParameters(
        Property Property,
        int Iterations,
        int ShrinkLimit,
        string? Replay,
        int? Seed,
        int? Size);

    internal interface IPropertyRunner
    {
        void Run(PropertyRunParameters parameters, ITestOutputHelper testOutputHelper);
    }
}
