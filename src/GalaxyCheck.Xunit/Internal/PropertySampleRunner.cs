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

            parameters.Property.Advanced.Print(
                stdout: log.Add,
                iterations: parameters.Iterations);

            throw new SampleException(string.Join(Environment.NewLine, log));
        }
    }
}
