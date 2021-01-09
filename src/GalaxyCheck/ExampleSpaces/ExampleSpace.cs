using GalaxyCheck.Abstractions;
using GalaxyCheck.Utility;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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

        public abstract IEnumerable<IExampleSpace<T>> Subspace { get; }

        public abstract IExampleSpace<TResult> Select<TResult>(Func<T, TResult> selector);

        public abstract IExampleSpace<T> Where(Func<T, bool> pred);

        public abstract bool Any();

        public abstract Example<T>? Navigate(List<int> path);

        public abstract IEnumerable<LocatedExample<T>> Traverse();

        public abstract IEnumerable<Counterexample<T>> Counterexamples(Func<T, bool> pred);
    }

    public record PopulatedExampleSpace<T> : ExampleSpace<T>
    {
        private readonly Example<T> _current;
        private readonly IEnumerable<PopulatedExampleSpace<T>> _subspace;

        public PopulatedExampleSpace(Example<T> current, IEnumerable<PopulatedExampleSpace<T>> subspace)
        {
            _current = current;
            _subspace = subspace;
        }

        public override IEnumerable<IExampleSpace<T>> Subspace => _subspace;

        public override IExampleSpace<TResult> Select<TResult>(Func<T, TResult> selector)
        {
            static PopulatedExampleSpace<TResult> SelectRec(
                PopulatedExampleSpace<T> exampleSpace,
                Func<T, TResult> selector) => new PopulatedExampleSpace<TResult>(
                    new Example<TResult>(selector(exampleSpace._current.Value), exampleSpace._current.Distance),
                    exampleSpace._subspace.Select(exampleSpace0 => SelectRec(exampleSpace0, selector)));

            return SelectRec(this, selector);
        }

        public override IExampleSpace<T> Where(Func<T, bool> pred)
        {
            if (pred(_current.Value) == false) return new EmptyExampleSpace<T>();

            static IEnumerable<PopulatedExampleSpace<T>> WhereRec(
                IEnumerable<PopulatedExampleSpace<T>> subspace,
                Func<T, bool> pred) => subspace
                    .Where(exampleSpace => pred(exampleSpace._current.Value))
                    .Select(exampleSpace => new PopulatedExampleSpace<T>(
                        exampleSpace._current, WhereRec(exampleSpace._subspace, pred)));

            return new PopulatedExampleSpace<T>(_current, WhereRec(_subspace, pred));
        }

        public override bool Any() => true;

        public override IEnumerable<LocatedExample<T>> Traverse()
        {
            static IEnumerable<LocatedExample<T>> TraverseRec(PopulatedExampleSpace<T> exampleSpace, int levelIndex, int siblingIndex)
            {
                yield return new LocatedExample<T>(
                    exampleSpace._current.Value,
                    exampleSpace._current.Distance,
                    levelIndex,
                    siblingIndex);

                var children = exampleSpace._subspace.SelectMany((childExampleSpace, siblingIndex) =>
                    TraverseRec(childExampleSpace, levelIndex + 1, siblingIndex));

                foreach (var child in children)
                {
                    yield return child;
                }
            }

            return TraverseRec(this, 0, 0);
        }

        public override IEnumerable<Counterexample<T>> Counterexamples(Func<T, bool> pred)
        {
            IEnumerable<Counterexample<T>> CounterexamplesRec(IEnumerable<PopulatedExampleSpace<T>> exampleSpaces)
            {
                var exampleSpaceAndIndex = exampleSpaces
                    .Select((exampleSpace, index) => new { exampleSpace, index })
                    .Where(x => pred(x.exampleSpace._current.Value) == false)
                    .FirstOrDefault();

                if (exampleSpaceAndIndex == null) yield break;

                var counterexample = new Counterexample<T>(
                    exampleSpaceAndIndex.exampleSpace._current.Value,
                    exampleSpaceAndIndex.exampleSpace._current.Distance,
                    new[] { exampleSpaceAndIndex.index });

                yield return counterexample;

                foreach (var smallerCounterexample in CounterexamplesRec(exampleSpaceAndIndex.exampleSpace._subspace))
                {
                    yield return new Counterexample<T>(
                        smallerCounterexample.Value,
                        smallerCounterexample.Distance,
                        Enumerable.Concat(counterexample.Path, smallerCounterexample.Path));
                }
            }

            return CounterexamplesRec(new[] { this });
        }

        public override Example<T>? Navigate(List<int> path)
        {
            static Example<T>? NavigateRec(PopulatedExampleSpace<T> exampleSpace, List<int> path)
            {
                if (path.Any() == false) return exampleSpace._current;

                var nextExampleSpace = exampleSpace._subspace.Skip(path.First()).FirstOrDefault();

                if (nextExampleSpace == null) return null;

                return NavigateRec(nextExampleSpace, path.Skip(1).ToList());
            }

            return NavigateRec(this, path.Skip(1).ToList());
        }
    }

    public record EmptyExampleSpace<T>() : ExampleSpace<T>
    {
        public override IEnumerable<IExampleSpace<T>> Subspace => Enumerable.Empty<IExampleSpace<T>>();

        public override IExampleSpace<TResult> Select<TResult>(Func<T, TResult> selector) => new EmptyExampleSpace<TResult>();

        public override IExampleSpace<T> Where(Func<T, bool> _) => new EmptyExampleSpace<T>();

        public override bool Any() => false;

        public override IEnumerable<LocatedExample<T>> Traverse() => Enumerable.Empty<LocatedExample<T>>();

        public override IEnumerable<Counterexample<T>> Counterexamples(Func<T, bool> pred) => Enumerable.Empty<Counterexample<T>>();

        public override Example<T>? Navigate(List<int> path) => null;
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
            UnfoldInternal(rootValue, shrink, measure, x => x, ImmutableHashSet.Create<T>(rootValue));

        private static PopulatedExampleSpace<TProjection> UnfoldInternal<TAccumulator, TProjection>(
            TAccumulator accumulator,
            ShrinkFunc<TAccumulator> shrink,
            MeasureFunc<TAccumulator> measure,
            Func<TAccumulator, TProjection> projection,
            ImmutableHashSet<TAccumulator> encountered)
        {
            var value = projection(accumulator);
            var distance = measure(accumulator);
            return new PopulatedExampleSpace<TProjection>(
                new Example<TProjection>(value, distance),
                UnfoldSubspaceInternal(shrink(accumulator), shrink, measure, projection, encountered));
        }

        private static IEnumerable<PopulatedExampleSpace<TProjection>> UnfoldSubspaceInternal<TAccumulator, TProjection>(
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
                    .Cast<PopulatedExampleSpace<TProjection>>();

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
