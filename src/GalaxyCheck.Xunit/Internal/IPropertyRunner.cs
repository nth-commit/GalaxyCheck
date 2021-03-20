using Xunit.Abstractions;

namespace GalaxyCheck.Xunit.Internal
{
    internal record PropertyRunParameters(
        Property<object> Property,
        int Iterations,
        int ShrinkLimit,
        string? Replay);

    internal interface IPropertyRunner
    {
        void Run(PropertyRunParameters parameters, ITestOutputHelper testOutputHelper);
    }
}
