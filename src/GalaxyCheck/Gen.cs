using GalaxyCheck.Abstractions;
using GalaxyCheck.ExampleSpaces;
using GalaxyCheck.Gens;
using GalaxyCheck.Runners;
using System;
using System.Collections.Generic;

namespace GalaxyCheck
{
    public static class Gen
    {
        /// <summary>
        /// Creates a generator that always produces the given value.
        /// </summary>
        /// <typeparam name="T">The type of the generator's value.</typeparam>
        /// <param name="value">The constant value the generator should produce.</param>
        /// <returns>The new generator.</returns>
        public static IGenBuilder<T> Constant<T>(T value) => PrimitiveGenBuilder.Create(
            (useNextInt) => value,
            ShrinkFunc.None<T>(),
            MeasureFunc.Unmeasured<T>());

        /// <summary>
        /// Creates a generator that produces 32-bit integers. By default, it will generate integers in the full range
        /// (-2,147,483,648 to 2,147,483,648), but the generator returned contains configuration methods to constrain
        /// the produced integers further.
        /// </summary>
        /// <returns></returns>
        public static IIntegerGenBuilder Int32() => IntegerGenBuilder.Create();

        /// <summary>
        /// Enumerates a generator to produce a sample of it's values.
        /// </summary>
        /// <typeparam name="T">The type of the generator's value.</typeparam>
        /// <param name="gen">The generator to sample.</param>
        /// <param name="configure">An optional configuration function for the sample.</param>
        /// <returns>A list of values produced by the generator.</returns>
        public static List<T> Sample<T>(this IGen<T> gen, Func<SampleOptions, SampleOptions>? configure = null) =>
            gen.SampleWithMetrics(configure).Values;
    }
}
