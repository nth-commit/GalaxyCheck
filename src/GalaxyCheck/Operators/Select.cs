using GalaxyCheck.Internal.ExampleSpaces;
using GalaxyCheck.Internal.GenIterations;
using GalaxyCheck.Internal.Gens;
using GalaxyCheck.Internal.Utility;
using System;

namespace GalaxyCheck
{
    public static class SelectExtensions
    {
        /// <summary>
        /// Projects each element in a generator to a new form.
        /// </summary>
        /// <typeparam name="T">The type of the generator's value.</typeparam>
        /// <typeparam name="TResult">The type of the resultant generator's value.</typeparam>
        /// <param name="gen">The generator to apply the projection to.</param>
        /// <param name="selector">A projection function to apply to each value.</param>
        /// <returns>A new generator with the projection applied.</returns>
        /// <summary>
        /// Projects each element in a generator to a new form.
        /// </summary>
        /// <typeparam name="T">The type of the generator's value.</typeparam>
        /// <typeparam name="TResult">The type of the resultant generator's value.</typeparam>
        /// <param name="gen">The generator to apply the projection to.</param>
        /// <param name="selector">A projection function to apply to each value.</param>
        /// <returns>A new generator with the projection applied.</returns>
        public static IGen<TResult> Select<T, TResult>(this IGen<T> gen, Func<T, TResult> selector)
        {
            GenInstanceTransformation<T, TResult> transformation = (instance) => {
                var sourceExampleSpace = instance.ExampleSpace;
                var projectedExampleSpace = instance.ExampleSpace.Map(selector);

                return GenIterationFactory.Instance(
                    instance.RepeatParameters,
                    instance.NextParameters,
                    projectedExampleSpace,
                    instance.ExampleSpaceHistory);
            };

            return gen.TransformInstances(transformation);
        }
    }

    public static partial class Gen
    {
        public static IGen<TResult> Select<T0, TResult>(
            IGen<T0> gen0,
            Func<T0, TResult> selector) =>
                from x0 in gen0
                select selector(x0);

        public static IGen<TResult> Select<T0, T1, TResult>(
            IGen<T0> gen0,
            IGen<T1> gen1,
            Func<T0, T1, TResult> selector) =>
                from x0 in gen0
                from x1 in gen1
                select selector(x0, x1);

        public static IGen<TResult> Select<T0, T1, T2, TResult>(
            IGen<T0> gen0,
            IGen<T1> gen1,
            IGen<T2> gen2,
            Func<T0, T1, T2, TResult> selector) =>
                from x0 in gen0
                from x1 in gen1
                from x2 in gen2
                select selector(x0, x1, x2);

        public static IGen<TResult> Select<T0, T1, T2, T3, TResult>(
            IGen<T0> gen0,
            IGen<T1> gen1,
            IGen<T2> gen2,
            IGen<T3> gen3,
            Func<T0, T1, T2, T3, TResult> selector) =>
                from x0 in gen0
                from x1 in gen1
                from x2 in gen2
                from x3 in gen3
                select selector(x0, x1, x2, x3);
    }
}
