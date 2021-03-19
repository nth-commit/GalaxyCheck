using Xunit.Abstractions;

namespace GalaxyCheck.Xunit.Internal
{
    public record PropertyRunParameters(
        Property<object> Property,
        int Iterations,
        int ShrinkLimit,
        string? Replay);

    public interface IPropertyRunner
    {
        void Run(PropertyRunParameters parameters, ITestOutputHelper testOutputHelper);
    }
}
