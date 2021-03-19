using GalaxyCheck;
using GalaxyCheck.Gens.Internal.Iterations;
using GalaxyCheck.Gens.Iterations.Generic;
using GalaxyCheck.Internal.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GalaxyCheck.Gens.Internal
{
    internal delegate IGen<U> GenTransformation<T, U>(IGen<T> gen);

    internal delegate IEnumerable<IGenIteration<U>> GenStreamTransformation<T, U>(IEnumerable<IGenIteration<T>> stream);

    internal delegate IGenIteration<U> GenIterationTransformation<T, U>(IGenIteration<T> iteration);

    internal delegate IGenIteration<U> GenInstanceTransformation<T, U>(IGenInstance<T> instance);

    internal static class GenTransformingExtensions
    {
        public static IGen<U> Transform<T, U>(this IGen<T> gen, GenTransformation<T, U> transformation) =>
            transformation(gen);

        public static IGen<U> TransformStream<T, U>(this IGen<T> gen, GenStreamTransformation<T, U> transformation) =>
            new FunctionalGen<U>((parameters) => transformation(gen.Advanced.Run(parameters)));

        public static IGen<U> TransformIterations<T, U>(
            this IGen<T> gen,
            GenIterationTransformation<T, U> transformation) =>
                gen.TransformStream(stream => stream.Select(transformation.Invoke));

        public static IGen<U> TransformInstances<T, U>(
            this IGen<T> gen,
            GenInstanceTransformation<T, U> transformation) =>
                gen.TransformIterations((IGenIteration<T> iteration) =>
                {
                    var either = iteration.ToEither<T, U>();

                    if (EitherExtension.IsLeft(either, out IGenInstance<T> instance))
                    {
                        return transformation(instance);
                    }
                    else if (EitherExtension.IsRight(either, out IGenIteration<U> iterationConverted))
                    {
                        return iterationConverted;
                    }
                    else
                    {
                        throw new Exception("Fatal: Unhandled branch");
                    }
                });

        public static IGen<T> Repeat<T>(this IGen<T> gen) => gen.Transform(GenTransformations.Repeat<T>());
    }

    internal static class GenTransformations
    {
        public static GenTransformation<T, T> Repeat<T>() => (gen) => new FunctionalGen<T>((parameters) =>
        {
            return EnumerableExtensions
                .Repeat(() => gen.Advanced.Run(parameters))
                .Tap((iteration) =>
                {
                    parameters = iteration.NextParameters;
                });
        });
    }
}
