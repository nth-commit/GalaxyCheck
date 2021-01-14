﻿using GalaxyCheck;
using GalaxyCheck.Internal.Utility;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GalaxyCheck.Internal.ExampleSpaces
{
    /// <summary>
    /// Represents an space of examples. An example space generally has an original or root value, which can be
    /// explored recursively, and the space itself is a tree-like structure to enable efficient exploration of the
    /// example space.
    /// </summary>
    /// <typeparam name="T">The type of an example's value</typeparam>
    public record ExampleSpace<T>
    {
        public Example<T> Current { get; init; }

        public IEnumerable<ExampleSpace<T>> Subspace { get; init; }

        public ExampleSpace(Example<T> current, IEnumerable<ExampleSpace<T>> subspace)
        {
            Current = current;
            Subspace = subspace;
        }
    }

    public static class ExampleSpaceExtensions
    {
        /// <summary>
        /// Maps all of the example values in an example space by the given selector function.
        /// </summary>
        /// <typeparam name="T">The type of the target example space's values.</typeparam>
        /// <typeparam name="TResult">The new type of an example's value</typeparam>
        /// <param name="exampleSpace">The example space to operate on.</param>
        /// <param name="selector">A function to apply to each value in the example space.</param>
        /// <returns>A new example space with the mapping function applied.</returns>
        public static ExampleSpace<TResult> Map<T, TResult>(
            this ExampleSpace<T> exampleSpace,
            Func<T, TResult> f)
        {
            return exampleSpace.MapExamples(example => new Example<TResult>(
                example.Id,
                f(example.Value),
                example.Distance));
        }

        /// <summary>
        /// Maps all of the examples in an example space by the given selector function.
        /// </summary>
        /// <typeparam name="T">The type of the target example space's values.</typeparam>
        /// <typeparam name="TResult">The new type of an example's value</typeparam>
        /// <param name="exampleSpace">The example space to operate on.</param>
        /// <param name="selector">A function to apply to each example in the example space.</param>
        /// <returns>A new example space with the mapping  function applied.</returns>
        public static ExampleSpace<TResult> MapExamples<T, TResult>(
            this ExampleSpace<T> exampleSpace,
            Func<Example<T>, Example<TResult>> f)
        {
            return new ExampleSpace<TResult>(
                f(exampleSpace.Current),
                exampleSpace.Subspace.Select(es => MapExamples(es, f)));
        }

        /// <summary>
        /// Filters the examples in an example space by the given predicate.
        /// </summary>
        /// <typeparam name="T">The type of the target example space's values.</typeparam>
        /// <param name="exampleSpace">The example space to operate on.</param>
        /// <param name="pred">The predicate used to test each value in the example space.</param>
        /// <returns>A new example space, containing only the examples whose values passed the predicate.</returns>
        public static ExampleSpace<T>? Filter<T>(
            this ExampleSpace<T> exampleSpace,
            Func<T, bool> pred)
        {
            if (pred(exampleSpace.Current.Value) == false) return null;

            return new ExampleSpace<T>(
                exampleSpace.Current,
                exampleSpace.Subspace
                    .Select(es => es.Filter(pred))
                    .Where(es => es != null)
                    .Cast<ExampleSpace<T>>());
        }

        /// <summary>
        /// Returns an enumerable of increasingly smaller counterexamples to the given predicate. An empty enumerable
        /// indicates that no counterexamples exist in this example space.
        /// </summary>
        /// <typeparam name="T">The type of the target example space's values.</typeparam>
        /// <param name="exampleSpace">The example space to operate on.</param>
        /// <param name="pred">The predicate used to test an example. If the predicate returns `false` for an example, it
        /// indicates that example is a counterexample.</param>
        /// <returns>An enumerable of counterexamples.</returns>
        public static IEnumerable<Counterexample<T>> Counterexamples<T>(
            this ExampleSpace<T> exampleSpace,
            Func<T, bool> pred)
        {
            (bool success, Exception? exception) Invoke(T value) {
                try
                {
                    var success = pred(value);
                    return (success, null);
                }
                catch (Exception ex)
                {
                    return (false, ex);
                }
            }

            IEnumerable<Counterexample<T>> CounterexamplesRec(IEnumerable<ExampleSpace<T>> exampleSpaces)
            {
                var failure = exampleSpaces
                    .Select((exampleSpace, index) =>
                    {
                        var (success, exception) = Invoke(exampleSpace.Current.Value);
                        return new { exampleSpace, index, success, exception };
                    })
                    .Where(x => x.success == false)
                    .FirstOrDefault();

                if (failure == null) yield break;

                var counterexample = new Counterexample<T>(
                    failure.exampleSpace.Current.Id,
                    failure.exampleSpace.Current.Value,
                    failure.exampleSpace.Current.Distance,
                    failure.exception,
                    new[] { failure.index });

                yield return counterexample;

                foreach (var smallerCounterexample in CounterexamplesRec(failure.exampleSpace.Subspace))
                {
                    yield return new Counterexample<T>(
                        smallerCounterexample.Id,
                        smallerCounterexample.Value,
                        smallerCounterexample.Distance,
                        smallerCounterexample.Exception,
                        counterexample.Path.Concat(smallerCounterexample.Path));
                }
            }

            return CounterexamplesRec(new[] { exampleSpace });
        }

        public static Example<T>? Navigate<T>(this ExampleSpace<T> exampleSpace, List<int> path)
        {
            static Example<T>? NavigateRec(ExampleSpace<T> exampleSpace, List<int> path)
            {
                if (path.Any() == false) return exampleSpace.Current;

                var nextExampleSpace = exampleSpace.Subspace.Skip(path.First()).FirstOrDefault();

                if (nextExampleSpace == null) return null;

                return NavigateRec(nextExampleSpace, path.Skip(1).ToList());
            }

            return NavigateRec(exampleSpace, path.Skip(1).ToList());
        }
    }

    public record Counterexample<T> : Example<T>
    {
        public IEnumerable<int> Path { get; init; }

        public Exception? Exception { get; init; }

        public Counterexample(
            ExampleId id,
            T value,
            decimal distance,
            Exception? exception,
            IEnumerable<int> path)
            : base(id, value, distance)
        {
            Path = path;
            Exception = exception;
        }
    }

    public static class ExampleSpace
    {
        /// <summary>
        /// Creates an example space containing a single example.
        /// </summary>
        /// <typeparam name="T">The type of the example's value.</typeparam>
        /// <param name="value">The example value.</param>
        /// <returns>The example space.</returns>
        public static ExampleSpace<T> Singleton<T>(T value) => new ExampleSpace<T>(
            new Example<T>(ExampleId.Empty, value, 0),
            Enumerable.Empty<ExampleSpace<T>>());

        /// <summary>
        /// Creates an example space by recursively applying a shrinking function to the root value.
        /// </summary>
        /// <typeparam name="T">The type of the example's value.</typeparam>
        /// <param name="rootValue"></param>
        /// <param name="shrink"></param>
        /// <param name="measure"></param>
        /// <returns></returns>
        public static ExampleSpace<T> Unfold<T>(
            T rootValue,
            ShrinkFunc<T> shrink,
            MeasureFunc<T> measure,
            IdentifyFunc<T> identify)
        {
            var id = identify(rootValue);
            return UnfoldInternal(
                rootValue,
                shrink,
                measure,
                identify,
                x => x,
                ImmutableHashSet.Create(id));
        }

        private static ExampleSpace<TProjection> UnfoldInternal<TAccumulator, TProjection>(
            TAccumulator accumulator,
            ShrinkFunc<TAccumulator> shrink,
            MeasureFunc<TAccumulator> measure,
            IdentifyFunc<TAccumulator> identify,
            Func<TAccumulator, TProjection> projection,
            ImmutableHashSet<ExampleId> encountered)
        {
            var id = identify(accumulator);
            var value = projection(accumulator);
            var distance = measure(accumulator);
            return new ExampleSpace<TProjection>(
                new Example<TProjection>(id, value, distance),
                UnfoldSubspaceInternal(shrink(accumulator), shrink, measure, identify, projection, encountered));
        }

        private static IEnumerable<ExampleSpace<TProjection>> UnfoldSubspaceInternal<TAccumulator, TProjection>(
            IEnumerable<TAccumulator> accumulators,
            ShrinkFunc<TAccumulator> shrink,
            MeasureFunc<TAccumulator> measure,
            IdentifyFunc<TAccumulator> identify,
            Func<TAccumulator, TProjection> projection,
            ImmutableHashSet<ExampleId> encountered)
        {
            return TraverseUnencountered(
                accumulators,
                (accumulator, encountered) =>
                    UnfoldInternal(accumulator, shrink, measure, identify, projection, encountered),
                encountered);
        }

        public static ExampleSpace<TResult> Merge<T, TResult>(
            IEnumerable<ExampleSpace<T>> exampleSpaces,
            Func<IEnumerable<T>, TResult> mergeValues,
            ShrinkFunc<IEnumerable<ExampleSpace<T>>> shrinkExampleSpaces,
            MeasureFunc<IEnumerable<ExampleSpace<T>>> measureMerge) =>
                MergeInternal(
                    exampleSpaces,
                    mergeValues,
                    shrinkExampleSpaces,
                    measureMerge,
                    ImmutableHashSet.Create<ExampleId>());

        private static ExampleSpace<TResult> MergeInternal<T, TResult>(
            IEnumerable<ExampleSpace<T>> exampleSpaces,
            Func<IEnumerable<T>, TResult> mergeValues,
            ShrinkFunc<IEnumerable<ExampleSpace<T>>> shrinkExampleSpaces,
            MeasureFunc<IEnumerable<ExampleSpace<T>>> measureMerge,
            ImmutableHashSet<ExampleId> encountered)
        {
            var current = new Example<TResult>(
                exampleSpaces.Aggregate(ExampleId.Empty, (acc, curr) => ExampleId.Combine(acc, curr.Current.Id)),
                mergeValues(exampleSpaces.Select(es => es.Current.Value)),
                measureMerge(exampleSpaces));

            var exampleSpaceCullingShrinks = shrinkExampleSpaces(exampleSpaces);

            var subspaceMergingShrinks = exampleSpaces
                .Select((exampleSpace, index) => LiftAndInsertSubspace(exampleSpaces, exampleSpace.Subspace, index))
                .SelectMany(exampleSpaces => exampleSpaces);

            var shrinks = TraverseUnencountered(
                Enumerable.Concat(exampleSpaceCullingShrinks, subspaceMergingShrinks),
                (exampleSpaces, encountered) =>
                    MergeInternal(exampleSpaces, mergeValues, shrinkExampleSpaces, measureMerge, encountered),
                encountered);

            return new ExampleSpace<TResult>(current, shrinks);
        }

        private static IEnumerable<IEnumerable<ExampleSpace<T>>> LiftAndInsertSubspace<T>(
            IEnumerable<ExampleSpace<T>> exampleSpaces,
            IEnumerable<ExampleSpace<T>> subspace,
            int index)
        {
            var leftExampleSpaces = exampleSpaces.Take(index);
            var rightExampleSpaces = exampleSpaces.Skip(index + 1);

            return subspace.Select(exampleSpace =>
                Enumerable.Concat(
                    leftExampleSpaces,
                    Enumerable.Concat(new[] { exampleSpace }, rightExampleSpaces)));
        }

        private static IEnumerable<ExampleSpace<T>> TraverseUnencountered<TAccumulator, T>(
            IEnumerable<TAccumulator> accumulators,
            Func<TAccumulator, ImmutableHashSet<ExampleId>, ExampleSpace<T>> generateNextExampleSpace,
            ImmutableHashSet<ExampleId> encountered)
        {
            return accumulators
                .Scan(
                    new UnfoldSubspaceState<ExampleSpace<T>>(
                        new Option.None<ExampleSpace<T>>(),
                        encountered),
                    (acc, curr) =>
                    {
                        var exampleSpace = generateNextExampleSpace(curr, acc.Encountered);

                        var hasBeenEncountered = acc.Encountered.Contains(exampleSpace.Current.Id);
                        if (hasBeenEncountered)
                        {
                            return new UnfoldSubspaceState<ExampleSpace<T>>(
                                new Option.None<ExampleSpace<T>>(),
                                acc.Encountered);
                        }

                        return new UnfoldSubspaceState<ExampleSpace<T>>(
                                new Option.Some<ExampleSpace<T>>(exampleSpace),
                                acc.Encountered.Add(exampleSpace.Current.Id));
                    }
                )
                .Select(x => x.UnencounteredValue switch
                {
                    Option.None<ExampleSpace<T>> _ => null,
                    Option.Some<ExampleSpace<T>> some => some.Value,
                    _ => null
                })
                .Where(exampleSpace => exampleSpace != null)
                .Cast<ExampleSpace<T>>();
        }

        private class UnfoldSubspaceState<T>
        {
            public Option<T> UnencounteredValue { get; init; }

            public ImmutableHashSet<ExampleId> Encountered { get; init; }

            public UnfoldSubspaceState(Option<T> unencounteredValue, ImmutableHashSet<ExampleId> encountered)
            {
                UnencounteredValue = unencounteredValue;
                Encountered = encountered;
            }
        }
    }
} 