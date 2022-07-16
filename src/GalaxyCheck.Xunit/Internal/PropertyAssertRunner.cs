using System;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace GalaxyCheck.Xunit.Internal
{
    internal class PropertyAssertRunner : IPropertyRunner
    {
        public Task Run(PropertyRunParameters parameters, ITestOutputHelper testOutputHelper) =>
            parameters.Property.AssertAsync(
                replay: parameters.Replay,
                seed: parameters.Seed,
                size: parameters.Size,
                iterations: parameters.Iterations,
                shrinkLimit: parameters.ShrinkLimit,
                formatReproduction: (newReplay) => $"{Environment.NewLine}    [Replay(\"{newReplay}\")]",
                formatMessage: (x) => Environment.NewLine + Environment.NewLine + x);
    }
}
