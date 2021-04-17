using Xunit.Abstractions;

namespace GalaxyCheck.Xunit.Internal
{
    internal record PropertyRunParameters(
        IGen<Test> Property,
        int Iterations,
        int ShrinkLimit,
        string? Replay);

    internal interface IPropertyRunner
    {
        void Run(PropertyRunParameters parameters, ITestOutputHelper testOutputHelper);
    }
}
