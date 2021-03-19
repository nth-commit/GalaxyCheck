﻿using System;
using Xunit.Abstractions;

namespace GalaxyCheck.Xunit.Internal
{
    public class PropertyAssertRunner : IPropertyRunner
    {
        public void Run(PropertyRunParameters parameters, ITestOutputHelper testOutputHelper)
        {
            parameters.Property.Assert(
                replay: parameters.Replay,
                iterations: parameters.Iterations,
                shrinkLimit: parameters.ShrinkLimit,
                formatReproduction: (newReplay) => $"{Environment.NewLine}    [Replay(\"{newReplay}\")]",
                formatMessage: (x) => Environment.NewLine + Environment.NewLine + x);
        }
    }
}