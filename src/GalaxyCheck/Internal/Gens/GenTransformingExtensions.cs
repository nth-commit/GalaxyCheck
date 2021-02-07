﻿using GalaxyCheck;
using GalaxyCheck.Internal.GenIterations;
using GalaxyCheck.Internal.Utility;
using System.Collections.Generic;
using System.Linq;

namespace GalaxyCheck.Internal.Gens
{
    public delegate IGen<U> GenTransformation<T, U>(IGen<T> gen);

    public delegate IEnumerable<IGenIteration<U>> GenStreamTransformation<T, U>(IEnumerable<IGenIteration<T>> stream);

    public delegate IGenIteration<U> GenIterationTransformation<T, U>(IGenIteration<T> iteration);

    public delegate IGenIteration<U> GenInstanceTransformation<T, U>(GenInstance<T> instance);

    public static class GenTransformingExtensions
    {
        public static IGen<U> Transform<T, U>(this IGen<T> gen, GenTransformation<T, U> transformation) =>
            transformation(gen);

        public static IGen<U> TransformStream<T, U>(this IGen<T> gen, GenStreamTransformation<T, U> transformation) =>
            new FunctionalGen<U>((rng, size) => transformation(gen.Advanced.Run(rng, size)));

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

        public static IGen<T> Repeat<T>(this IGen<T> gen) => gen.Transform(GenTransformations.Repeat<T>());
    }

    public static class GenTransformations
    {
        public static GenTransformation<T, T> Repeat<T>() => (gen) => new FunctionalGen<T>((rng, size) =>
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
