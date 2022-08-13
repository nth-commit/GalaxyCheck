using GalaxyCheck.Gens.Internal;
using GalaxyCheck.Gens.Internal.Iterations;
using GalaxyCheck.ExampleSpaces;
using System;

namespace GalaxyCheck
{
    public static partial class Extensions
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
                    instance.ReplayParameters,
                    instance.NextParameters,
                    projectedExampleSpace);
            };

            return gen.TransformInstances(transformation);
        }
    }
}
