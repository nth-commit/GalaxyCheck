using System;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace GalaxyCheck.Xunit.Internal
{
    internal class PropertyAssertRunner : IPropertyRunner
    {
        public async Task Run(PropertyRunParameters parameters, ITestOutputHelper testOutputHelper)
        {
            await parameters.Property.Select(test => test.Cast<object>()).AssertAsync(
                replay: parameters.Replay,
                seed: parameters.Seed,
                size: parameters.Size,
                iterations: parameters.Iterations,
                shrinkLimit: parameters.ShrinkLimit,
                formatReproduction: (newReplay) => $"{Environment.NewLine}    [Replay(\"{newReplay}\")]",
                formatMessage: (x) => Environment.NewLine + Environment.NewLine + x);
        }
    }
}
