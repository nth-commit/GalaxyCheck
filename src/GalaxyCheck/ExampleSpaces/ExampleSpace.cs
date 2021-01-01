using System;
using System.Collections.Generic;
using System.Linq;

namespace GalaxyCheck.ExampleSpaces
{
    public record Example<T>(T Value, int Distance);

    public record ExampleSpace<TSource, TResult>(
        TSource Root,
        ShrinkFunc<TSource> Shrink,
        MeasureFunc<TSource> Measure,
        TransformFunc<TSource, TResult> Transform);

    public static class ExampleSpace
    {
        public static ExampleSpace<T, T> Singleton<T>(T value) => new ExampleSpace<T, T>(
            value,
            ShrinkFunc.None<T>(),
            MeasureFunc.Unmeasured<T>(),
            TransformShrinksFunc.Identity<T>());

        public static ExampleSpace<T, T> Unfold<T>(T rootValue, ShrinkFunc<T> shrink, MeasureFunc<T> measure) => new ExampleSpace<T, T>(
            rootValue,
            shrink,
            measure,
            TransformShrinksFunc.Identity<T>());

        public static ExampleSpace<TSource, TResult> Where<TSource, TResult>(
            this ExampleSpace<TSource, TResult> exampleSpace,
            Func<TResult, bool> pred) => new ExampleSpace<TSource, TResult>(
                exampleSpace.Root,
                exampleSpace.Shrink,
                exampleSpace.Measure,
                exampleSpace.Transform.ThenWhere(pred));

        public static ExampleSpace<TSource, TProjection> Select<TSource, TResult, TProjection>(
            this ExampleSpace<TSource, TResult> exampleSpace,
            Func<TResult, TProjection> selector) => new ExampleSpace<TSource, TProjection>(
                exampleSpace.Root,
                exampleSpace.Shrink,
                exampleSpace.Measure,
                exampleSpace.Transform.ThenSelect(selector));

        public static bool Any<TSource, TResult>(this ExampleSpace<TSource, TResult> exampleSpace) =>
            exampleSpace.Transform(exampleSpace.Root).Any();

        public static IEnumerable<Example<TResult>> Traverse<TSource, TResult>(this ExampleSpace<TSource, TResult> exampleSpace)
        {
            IEnumerable<Example<TResult>> traverseRec(TSource value)
            {
                foreach (var transformedValue in exampleSpace.Transform(value))
                {
                    yield return new Example<TResult>(transformedValue, exampleSpace.Measure(value));
                }

                foreach (var childExample in exampleSpace.Shrink(value).SelectMany(traverseRec))
                {
                    yield return childExample;
                }
            }

            return traverseRec(exampleSpace.Root);
        }
    }
}
