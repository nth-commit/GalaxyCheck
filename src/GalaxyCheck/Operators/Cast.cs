using GalaxyCheck.ExampleSpaces;
using GalaxyCheck.Gens.Internal;
using GalaxyCheck.Gens.Internal.Iterations;
using System.Linq;

namespace GalaxyCheck
{
    public static partial class Extensions
    {
        public static IGen<T> Cast<T>(this IGen gen) => new FunctionalGen<T>(parameters =>
            gen.Advanced
                .Run(parameters)
                .Select(iteration => iteration.Data.Match(
                    onInstance: instance => GenIterationFactory.Instance<T>(
                        iteration.ReplayParameters,
                        instance.NextParameters,
                        instance.ExampleSpace.Cast<T>()),
                    onError: error => GenIterationFactory.Error<T>(
                        iteration.ReplayParameters,
                        error.Message),
                    onDiscard: discard => GenIterationFactory.Discard<T>(
                        iteration.ReplayParameters,
                        discard.NextParameters,
                        discard.ExampleSpace))));

        public static IGen<T> Cast<T>(this IGen<T> gen)
        {
            IGen gen0 = gen;
            return gen0.Cast<T>();
        }
    }
}
