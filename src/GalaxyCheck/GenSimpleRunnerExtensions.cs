using GalaxyCheck.Abstractions;
using GalaxyCheck.Runners;
using System.Collections.Generic;

namespace GalaxyCheck
{
    public static class GenSimpleRunnerExtensions
    {
        /// <summary>
        /// Enumerates a generator to produce a sample of it's values.
        /// </summary>
        /// <typeparam name="T">The type of the generator's value.</typeparam>
        /// <param name="gen">The generator to sample.</param>
        /// <returns>A list of values produced by the generator.</returns>
        public static List<T> Sample<T>(this IGen<T> gen)
        {
            return gen.SampleWithMetrics().Values;
        }
    }
}
