using GalaxyCheck.Gens.Iterations.Generic;
using GalaxyCheck.Internal.Utility;

namespace GalaxyCheck.Gens.Internal.Iterations
{
    internal static class GenIterationExtensions
    {
        public static Either<IGenInstance<TSource>, IGenIteration<TNonInstance>> ToEither<TSource, TNonInstance>(
            this IGenIteration<TSource> iteration) => iteration.Match<Either<IGenInstance<TSource>, IGenIteration<TNonInstance>>>(
                onInstance: instance => new Left<IGenInstance<TSource>, IGenIteration<TNonInstance>>(instance),
                onError: error => new Right<IGenInstance<TSource>, IGenIteration<TNonInstance>>(
                    GenIterationFactory.Error<TNonInstance>(error.ReplayParameters, error.NextParameters, error.GenName, error.Message)),
                onDiscard: discard => new Right<IGenInstance<TSource>, IGenIteration<TNonInstance>>(
                    GenIterationFactory.Discard<TNonInstance>(discard.ReplayParameters, discard.NextParameters)));

        public static bool IsInstance<T>(this IGenIteration<T> iteration) => iteration.Match(
            onInstance: _ => true,
            onError: _ => false,
            onDiscard: _ => false);

        public static bool IsDiscard<T>(this IGenIteration<T> iteration) => iteration.Match(
            onInstance: _ => false,
            onError: _ => false,
            onDiscard: _ => true);
    }
}
