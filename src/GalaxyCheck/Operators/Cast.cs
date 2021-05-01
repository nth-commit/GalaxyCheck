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
                        iteration.NextParameters,
                        instance.ExampleSpace.Cast<T>(),
                        instance.ExampleSpaceHistory),
                    onError: error => GenIterationFactory.Error<T>(
                        iteration.ReplayParameters,
                        iteration.NextParameters,
                        error.GenName,
                        error.Message,
                        error.Error),
                    onDiscard: discard => GenIterationFactory.Discard<T>(
                        iteration.ReplayParameters,
                        iteration.NextParameters))));
    }
}
