using System;
using Xunit.Abstractions;

namespace GalaxyCheck.Xunit.Internal
{
    internal class PropertyAssertRunner : IPropertyRunner
    {
        public void Run(PropertyRunParameters parameters, ITestOutputHelper testOutputHelper)
        {
            parameters.Property.Select(test => test.Cast<object>()).Assert(
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
