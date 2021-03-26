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
                    onInstance: instance => GenIterationFactory.Instance(
                        iteration.ReplayParameters,
                        iteration.NextParameters,
                        ExampleSpaceExtensions.Cast<T>(instance.ExampleSpace),
                        instance.ExampleSpaceHistory),
                    onError: error => GenIterationFactory.Error<T>(
                        iteration.ReplayParameters,
                        iteration.NextParameters,
                        error.GenName,
                        error.Message),
                    onDiscard: discard => GenIterationFactory.Discard<T>(
                        iteration.ReplayParameters,
                        iteration.NextParameters))));
    }
}
