using GalaxyCheck.Utility;
using System.Collections.Generic;
using System.Linq;

namespace GalaxyCheck.Gens
{
    public delegate IGen<U> GenTransformation<T, U>(IGen<T> gen);

    public delegate IEnumerable<GenIteration<U>> GenStreamTransformation<T, U>(IEnumerable<GenIteration<T>> stream);

    public delegate GenIteration<U> GenIterationTransformation<T, U>(GenIteration<T> iteration);

    public delegate GenIteration<U> GenInstanceTransformation<T, U>(GenInstance<T> instance);

    public static class GenTransformingExtensions
    {
        public static IGen<U> Transform<T, U>(this IGen<T> gen, GenTransformation<T, U> transformation) =>
            transformation(gen);

        public static IGen<U> TransformStream<T, U>(this IGen<T> gen, GenStreamTransformation<T, U> transformation) =>
            new FunctionGen<U>((rng, size) => transformation(gen.Advanced.Run(rng, size)));

        public static IGen<U> TransformIterations<T, U>(
            this IGen<T> gen,
            GenIterationTransformation<T, U> transformation) =>
                gen.TransformStream(stream => stream.Select(transformation.Invoke));

        public static IGen<U> TransformInstances<T, U>(
            this IGen<T> gen,
            GenInstanceTransformation<T, U> transformation) =>
                gen.TransformIterations(iteration =>
                {
                    var iterationBuilder = GenIterationBuilder.FromIteration(iteration);
                    return iteration.Match(
                        onInstance: instance => transformation(instance),
                        onDiscard: discard => iterationBuilder.ToDiscard<U>(),
                        onError: error => iterationBuilder.ToError<U>(error.GenName, error.Message));
                });
    }

    public static class GenTransformations
    {
        public static GenTransformation<T, T> Repeat<T>() => (IGen<T> gen) => new FunctionGen<T>((rng, size) =>
        {
            return EnumerableExtensions
                .Repeat(() => gen.Advanced.Run(rng, size))
                .Tap((iteration) =>
                {
                    rng = iteration.NextRng;
                    size = iteration.NextSize;
                });
        });
    }
}
