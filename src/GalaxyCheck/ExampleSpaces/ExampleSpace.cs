using GalaxyCheck.Utility;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GalaxyCheck.ExampleSpaces
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
            IEnumerable<Counterexample<T>> CounterexamplesRec(IEnumerable<ExampleSpace<T>> exampleSpaces)
            {
                var exampleSpaceAndIndex = exampleSpaces
                    .Select((exampleSpace, index) => new { exampleSpace, index })
                    .Where(x => pred(x.exampleSpace.Current.Value) == false)
                    .FirstOrDefault();

                if (exampleSpaceAndIndex == null) yield break;

                var counterexample = new Counterexample<T>(
                    exampleSpaceAndIndex.exampleSpace.Current.Value,
                    exampleSpaceAndIndex.exampleSpace.Current.Distance,
                    new[] { exampleSpaceAndIndex.index });

                yield return counterexample;

                foreach (var smallerCounterexample in CounterexamplesRec(exampleSpaceAndIndex.exampleSpace.Subspace))
                {
                    yield return new Counterexample<T>(
                        smallerCounterexample.Value,
                        smallerCounterexample.Distance,
                        Enumerable.Concat(counterexample.Path, smallerCounterexample.Path));
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

    /// <summary>
    /// An example that lives inside an example space.
    /// </summary>
    /// <typeparam name="T">The type of the example's value.</typeparam>
    public record Example<T>
    {
        /// <summary>
        /// The example value.
        /// </summary>
        public T Value { get; init; }

        /// <summary>
        /// A metric which indicates how far the value is away from the smallest possible value. The metric is
        /// originally a proportion out of 100, but it composes when example spaces are composed. Therefore, it's
        /// possible for the distance metric to be arbitrarily high.
        /// </summary>
        public decimal Distance { get; init; }

        public Example(T value, decimal distance)
        {
            Value = value;
            Distance = distance;
        }
    }

    public record Counterexample<T> : Example<T>
    {
        public IEnumerable<int> Path { get; init; }

        public Counterexample(T value, decimal distance, IEnumerable<int> path) : base(value, distance)
        {
            Path = path;
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
            new Example<T>(value, 0),
            Enumerable.Empty<ExampleSpace<T>>());

        /// <summary>
        /// Creates an example space by recursively applying a shrinking function to the root value.
        /// </summary>
        /// <typeparam name="T">The type of the example's value.</typeparam>
        /// <param name="rootValue"></param>
        /// <param name="shrink"></param>
        /// <param name="measure"></param>
        /// <returns></returns>
        public static ExampleSpace<T> Unfold<T>(T rootValue, ShrinkFunc<T> shrink, MeasureFunc<T> measure) =>
            UnfoldInternal(rootValue, shrink, measure, x => x, ImmutableHashSet.Create<T>(rootValue));

        private static ExampleSpace<TProjection> UnfoldInternal<TAccumulator, TProjection>(
            TAccumulator accumulator,
            ShrinkFunc<TAccumulator> shrink,
            MeasureFunc<TAccumulator> measure,
            Func<TAccumulator, TProjection> projection,
            ImmutableHashSet<TAccumulator> encountered)
        {
            var value = projection(accumulator);
            var distance = measure(accumulator);
            return new ExampleSpace<TProjection>(
                new Example<TProjection>(value, distance),
                UnfoldSubspaceInternal(shrink(accumulator), shrink, measure, projection, encountered));
        }

        private static IEnumerable<ExampleSpace<TProjection>> UnfoldSubspaceInternal<TAccumulator, TProjection>(
            IEnumerable<TAccumulator> accumulators,
            ShrinkFunc<TAccumulator> shrink,
            MeasureFunc<TAccumulator> measure,
            Func<TAccumulator, TProjection> projection,
            ImmutableHashSet<TAccumulator> encountered) =>
                accumulators
                    .Scan(
                        new UnfoldSubspaceState<TAccumulator>(new Option.None<TAccumulator>(), encountered),
                        (acc, curr) =>
                        {
                            var hasBeenEncountered = acc.Encountered.Contains(curr);
                            return hasBeenEncountered
                                ? new UnfoldSubspaceState<TAccumulator>(new Option.None<TAccumulator>(), acc.Encountered)
                                : new UnfoldSubspaceState<TAccumulator>(new Option.Some<TAccumulator>(curr), acc.Encountered.Add(curr));
                        }
                    )
                    .Select(x => x.UnencounteredValue switch
                    {
                        Option.None<TAccumulator> _ => null,
                        Option.Some<TAccumulator> some => UnfoldInternal(some.Value, shrink, measure, projection, x.Encountered),
                        _ => null
                    })
                    .Where(exampleSpace => exampleSpace != null)
                    .Cast<ExampleSpace<TProjection>>();

        private class UnfoldSubspaceState<T>
        {
            public Option<T> UnencounteredValue { get; init; }

            public ImmutableHashSet<T> Encountered { get; init; }

            public UnfoldSubspaceState(Option<T> unencounteredValue, ImmutableHashSet<T> encountered)
            {
                UnencounteredValue = unencounteredValue;
                Encountered = encountered;
            }
        }
    }
}
