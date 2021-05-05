using GalaxyCheck.Gens;
using Xunit.Abstractions;

namespace GalaxyCheck.Xunit.Internal
{
    internal record PropertyRunParameters(
        IGen<Test<object>> Property,
        int Iterations,
        int ShrinkLimit,
        string? Replay);

    internal interface IPropertyRunner
    {
        void Run(PropertyRunParameters parameters, ITestOutputHelper testOutputHelper);
    }
}
