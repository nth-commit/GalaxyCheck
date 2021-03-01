using GalaxyCheck.Internal.ExampleSpaces;
using GalaxyCheck.Internal.GenIterations;
using GalaxyCheck.Internal.Gens;
using System.Linq;

namespace GalaxyCheck
{
    public static partial class Gen
    {
        public static IGen<T> Cast<T>(this IGen gen) => new FunctionalGen<T>(parameters =>
            gen.Advanced
                .Run(parameters)
                .Select(iteration => iteration.Data.Match(
                    onInstance: instance => GenIterationFactory.Instance(
                        iteration.RepeatParameters,
                        iteration.NextParameters,
                        ExampleSpaceExtensions.Cast<T>(instance.ExampleSpace)),
                    onError: error => GenIterationFactory.Error<T>(
                        iteration.RepeatParameters,
                        iteration.NextParameters,
                        error.GenName,
                        error.Message),
                    onDiscard: discard => GenIterationFactory.Discard<T>(
                        iteration.RepeatParameters,
                        iteration.NextParameters))));
    }
}
