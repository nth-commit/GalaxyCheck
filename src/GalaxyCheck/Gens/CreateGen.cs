using GalaxyCheck.ExampleSpaces;
using GalaxyCheck.Gens;
using GalaxyCheck.Gens.Internal;
using GalaxyCheck.Gens.Internal.Iterations;
using GalaxyCheck.Gens.Iterations.Generic;
using GalaxyCheck.Gens.Parameters;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace GalaxyCheck
{
    public static partial class Gen
    {
        /// <summary>
        /// Generates instances of the given type, using the default <see cref="IGenFactory"/>. The auto-generator
        /// can not be configured as precisely as more specialized generators can be, but it can create complex types
        /// with minimal configuration through reflection.
        /// </summary>
        /// <returns>A generator for the given type.</returns>
        public static IReflectedGen<T> Create<T>(NullabilityInfo? nullabilityInfo = null) where T : notnull => Factory().Create<T>(nullabilityInfo);

        /// <summary>
        /// Generates instances of the given type, using the default <see cref="IGenFactory"/>. The auto-generator
        /// can not be configured as precisely as more specialized generators can be, but it can create complex types
        /// with minimal configuration through reflection.
        /// </summary>
        /// <returns>A generator for the given type.</returns>
        public static IGen<object> Create(Type type, NullabilityInfo? nullabilityInfo = null) => Factory().Create(type, nullabilityInfo);

        /// <summary>
        /// Generates instances by the given function. Instances will not shrink by default, to enable shrinking on the
        /// generator, use <see cref="Extensions.Unfold{T}(IGen{T}, Func{T, IExampleSpace{T}})"/>. The function must
        /// return the parameters to be used in the next iteration, considering any randomness that was used in this
        /// to generate this iteration.
        /// </summary>
        /// <param name="func">The generator function.</param>
        /// <returns>A generator for the given type, create by the given function.</returns>
        public static IGen<T> Create<T>(Func<GenParameters, (T value, GenParameters nextParameters)> func)
        {
            IEnumerable<IGenIteration<T>> GenFunc(GenParameters parameters)
            {
                while (true)
                {
                    var replayParameters = parameters;
                    var (value, nextParameters) = func(replayParameters);
                    parameters = nextParameters;
                    yield return GenIterationFactory.Instance(replayParameters, nextParameters, ExampleSpaceFactory.Singleton(value));
                }
            }

            return new FunctionalGen<T>(GenFunc);
        }
    }
}
