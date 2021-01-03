﻿using GalaxyCheck.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GalaxyCheck.ExampleSpaces
{
    /// <summary>
    /// Represents an abstract space of examples. An example space generally has an original or root value, which can
    /// be explored recursively, and the space itself is a tree-like structure to enable efficient exploration of the
    /// example space.
    /// </summary>
    /// <typeparam name="T">The type of an example's value</typeparam>
    public abstract record ExampleSpace<T> : IExampleSpace<T>
    {
        internal ExampleSpace() { }

        /// <summary>
        /// Maps all of the examples in an example space by the given selector function.
        /// </summary>
        /// <typeparam name="TResult">The new type of an example's value</typeparam>
        /// <param name="selector">A function to apply to each value in the example space.</param>
        /// <returns>A new example space with the selector function applied.</returns>
        public abstract ExampleSpace<TResult> Select<TResult>(Func<T, TResult> selector);

        /// <summary>
        /// Filters the examples in an example space by the given predicate.
        /// </summary>
        /// <param name="pred">The predicate used to test each value in the example space.</param>
        /// <returns>A new example space, containing only the examples whose values passed the predicate.</returns>
        public abstract ExampleSpace<T> Where(Func<T, bool> pred);

        /// <summary>
        /// Returns a value indicating if there are any examples in the example space.
        /// </summary>
        /// <returns>`true` if so, `false` otherwise.</returns>
        public abstract bool Any();

        public abstract IEnumerable<LocatedExample<T>> Traverse();
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

        public override IEnumerable<LocatedExample<T>> Traverse()
        {
            static IEnumerable<LocatedExample<T>> TraverseRec(PopulatedExampleSpace<T> exampleSpace, int levelIndex, int siblingIndex)
            {
                yield return new LocatedExample<T>(
                    exampleSpace.Current.Value,
                    exampleSpace.Current.Distance,
                    levelIndex,
                    siblingIndex);

                var children = exampleSpace.Subspace.SelectMany((childExampleSpace, siblingIndex) =>
                    TraverseRec(childExampleSpace, levelIndex + 1, siblingIndex));

                foreach (var child in children)
                {
                    yield return child;
                }
            }

            return TraverseRec(this, 0, 0);
        }
    }

    public record EmptyExampleSpace<T>() : ExampleSpace<T>
    {
        public override ExampleSpace<TResult> Select<TResult>(Func<T, TResult> selector) => new EmptyExampleSpace<TResult>();

        public override ExampleSpace<T> Where(Func<T, bool> _) => new EmptyExampleSpace<T>();

        public override bool Any() => false;

        public override IEnumerable<LocatedExample<T>> Traverse() => Enumerable.Empty<LocatedExample<T>>();
    }

    public static class ExampleSpace
    {
        /// <summary>
        /// Creates an example space containing a single example.
        /// </summary>
        /// <typeparam name="T">The type of the example's value.</typeparam>
        /// <param name="value">The example value.</param>
        /// <returns>The example space.</returns>
        public static ExampleSpace<T> Singleton<T>(T value) => new PopulatedExampleSpace<T>(
            new Example<T>(value, 0),
            Enumerable.Empty<PopulatedExampleSpace<T>>());

        /// <summary>
        /// Creates an example space by recursively applying a shrinking function to the root value.
        /// </summary>
        /// <typeparam name="T">The type of the example's value.</typeparam>
        /// <param name="rootValue"></param>
        /// <param name="shrink"></param>
        /// <param name="measure"></param>
        /// <returns></returns>
        public static ExampleSpace<T> Unfold<T>(T rootValue, ShrinkFunc<T> shrink, MeasureFunc<T> measure) =>
            UnfoldInternal(rootValue, shrink, measure, x => x);

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
