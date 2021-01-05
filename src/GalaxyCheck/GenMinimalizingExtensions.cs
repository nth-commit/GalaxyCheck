using GalaxyCheck.Abstractions;
using System;
using System.Linq;

namespace GalaxyCheck.Aggregators
{
    public static class GenMinimalizingExtensions
    {
        public static T Minimal<T>(this IGen<T> gen, RunConfig? config = null)
        {
            var sample = gen.SampleExampleSpaces(config?.WithIterations(1));

            return sample.First().Minimal()!.Value;
        }
    }
}
