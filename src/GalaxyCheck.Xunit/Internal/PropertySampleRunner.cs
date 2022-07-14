using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace GalaxyCheck.Xunit.Internal
{
    internal class PropertySampleRunner : IPropertyRunner
    {
        public async Task Run(PropertyRunParameters parameters, ITestOutputHelper testOutputHelper)
        {
            var log = new List<string>();

            await parameters.Property.Advanced.PrintAsync(
                stdout: log.Add,
                seed: parameters.Seed,
                size: parameters.Size,
                iterations: parameters.Iterations);

            throw new SampleException(string.Join(Environment.NewLine, log));
        }
    }
}
