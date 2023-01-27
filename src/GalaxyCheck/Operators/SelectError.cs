using GalaxyCheck.Gens.Internal;
using GalaxyCheck.Gens.Internal.Iterations;
using GalaxyCheck.Gens.Iterations;
using GalaxyCheck.Gens.Iterations.Generic;
using System;

namespace GalaxyCheck
{
    public static partial class Extensions
    {
        internal static IGen<T> SelectError<T>(this IGen<T> gen, Func<IGenErrorData, IGenErrorData> errorSelector)
        {
            IGenIteration<T> SelectError(IGenError<T> error)
            {
                var selectedErrorData = errorSelector(error);
                return GenIterationFactory.Error<T>(
                    error.ReplayParameters,
                    error.NextParameters,
                    selectedErrorData.Message);
            }

            GenIterationTransformation<T, T> transformation = (iteration) =>
            {
                return iteration.Match(
                    onError: error => SelectError(error),
                    onInstance: instance => instance,
                    onDiscard: discard => discard);
            };

            return gen.TransformIterations(transformation);
        }
    }
}
