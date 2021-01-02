using System;
using System.Collections.Generic;
using System.Linq;

namespace GalaxyCheck.ExampleSpaces
{
    public record Example<T>(T Value, int Distance);

    public abstract record ExampleSpace<T>
    {
        internal ExampleSpace() { }

        public abstract ExampleSpace<TResult> Select<TResult>(Func<T, TResult> selector);

        public abstract ExampleSpace<T> Where(Func<T, bool> pred);

        public abstract bool Any();

        public abstract IEnumerable<Example<T>> Traverse();
    }

    public record PopulatedExampleSpace<T>(Example<T> Current, IEnumerable<PopulatedExampleSpace<T>> Subspace) : ExampleSpace<T>
    {
        public override ExampleSpace<TResult> Select<TResult>(Func<T, TResult> selector)
        {
            static PopulatedExampleSpace<TResult> SelectRec(
                PopulatedExampleSpace<T> exampleSpace,
                Func<T, TResult> selector) => new PopulatedExampleSpace<TResult>(
                    new Example<TResult>(selector(exampleSpace.Current.Value), exampleSpace.Current.Distance),
                    exampleSpace.Subspace.Select(exampleSpace0 => SelectRec(exampleSpace0, selector)));

            return SelectRec(this, selector);
        }

        public override ExampleSpace<T> Where(Func<T, bool> pred)
        {
            if (pred(Current.Value) == false) return new EmptyExampleSpace<T>();

            static IEnumerable<PopulatedExampleSpace<T>> WhereRec(
                IEnumerable<PopulatedExampleSpace<T>> subspace,
                Func<T, bool> pred) => subspace
                    .Where(exampleSpace => pred(exampleSpace.Current.Value))
                    .Select(exampleSpace => new PopulatedExampleSpace<T>(
                        exampleSpace.Current, WhereRec(exampleSpace.Subspace, pred)));

            return new PopulatedExampleSpace<T>(Current, WhereRec(Subspace, pred));
        }

        public override bool Any() => true;

        public override IEnumerable<Example<T>> Traverse()
        {
            yield return Current;

            foreach (var child in Subspace.SelectMany(exampleSpace => exampleSpace.Traverse()))
            {
                yield return child;
            }
        }
    }

    public record EmptyExampleSpace<T>() : ExampleSpace<T>
    {
        public override ExampleSpace<TResult> Select<TResult>(Func<T, TResult> selector) => new EmptyExampleSpace<TResult>();

        public override ExampleSpace<T> Where(Func<T, bool> _) => new EmptyExampleSpace<T>();

        public override bool Any() => false;

        public override IEnumerable<Example<T>> Traverse() => Enumerable.Empty<Example<T>>();
    }

    public static class ExampleSpace
    {
        public static ExampleSpace<T> Singleton<T>(T value) => new PopulatedExampleSpace<T>(
            new Example<T>(value, 0),
            Enumerable.Empty<PopulatedExampleSpace<T>>());

        public static ExampleSpace<T> Unfold<T>(T rootValue, ShrinkFunc<T> shrink, MeasureFunc<T> measure) =>
            Unfold(rootValue, shrink, measure, x => x);

        public static ExampleSpace<TProjection> Unfold<TAccumulator, TProjection>(
            TAccumulator accumulator,
            ShrinkFunc<TAccumulator> shrink,
            MeasureFunc<TAccumulator> measure,
            Func<TAccumulator, TProjection> projection) =>
                UnfoldInternal(accumulator, shrink, measure, projection);

        private static PopulatedExampleSpace<TProjection> UnfoldInternal<TAccumulator, TProjection>(
            TAccumulator accumulator,
            ShrinkFunc<TAccumulator> shrink,
            MeasureFunc<TAccumulator> measure,
            Func<TAccumulator, TProjection> projection)
        {
            var value = projection(accumulator);
            var distance = measure(accumulator);
            return new PopulatedExampleSpace<TProjection>(
                new Example<TProjection>(value, distance),
                UnfoldSubspaceInternal(shrink(accumulator), shrink, measure, projection));
        }

        private static IEnumerable<PopulatedExampleSpace<TProjection>> UnfoldSubspaceInternal<TAccumulator, TProjection>(
            IEnumerable<TAccumulator> accumulators,
            ShrinkFunc<TAccumulator> shrink,
            MeasureFunc<TAccumulator> measure,
            Func<TAccumulator, TProjection> projection) =>
                accumulators.Select(accumulator => UnfoldInternal(accumulator, shrink, measure, projection));
    }
}
