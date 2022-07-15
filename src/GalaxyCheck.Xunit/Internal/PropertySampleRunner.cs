using System;
using System.Collections.Generic;
using Xunit.Abstractions;

namespace GalaxyCheck.Xunit.Internal
{
    internal class PropertySampleRunner : IPropertyRunner
    {
        public void Run(PropertyRunParameters parameters, ITestOutputHelper testOutputHelper)
        {
            var log = new List<string>();

            parameters.Property.Print(
                stdout: log.Add,
                seed: parameters.Seed,
                size: parameters.Size,
                iterations: parameters.Iterations);

            throw new SampleException(string.Join(Environment.NewLine, log));
        }
    }
}
