using GalaxyCheck.Internal.Utility;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GalaxyCheck.Internal.ExampleSpaces
{
    public static class ExampleSpaceFactory
    {
        private record ExampleSpace<T> : IExampleSpace<T>
        {
            public IExample<T> Current { get; init; }

            public IEnumerable<IExampleSpace<T>> Subspace { get; init; }

            public ExampleSpace(IExample<T> current, IEnumerable<IExampleSpace<T>> subspace)
            {
                Current = current;
                Subspace = subspace;
            }

            IExample IExampleSpace.Current => Current;

            IEnumerable<IExampleSpace> IExampleSpace.Subspace => Subspace;
        }

        public static IExampleSpace<T> Create<T>(IExample<T> current, IEnumerable<IExampleSpace<T>> subspace) =>
            new ExampleSpace<T>(current, subspace);

        /// <summary>
        /// Creates an example space containing a single example.
        /// </summary>
        /// <typeparam name="T">The type of the example's value.</typeparam>
        /// <param name="value">The example value.</param>
        /// <returns>The example space.</returns>
        public static IExampleSpace<T> Singleton<T>(T value) => Singleton(ExampleId.Empty, value);

        public static IExampleSpace<T> Singleton<T>(ExampleId id, T value) => new ExampleSpace<T>(
            new Example<T>(id, value, 0),
            Enumerable.Empty<IExampleSpace<T>>());

        /// <summary>
        /// Creates an example space by recursively applying a shrinking function to the root value.
        /// </summary>
        /// <typeparam name="T">The type of the example's value.</typeparam>
        /// <param name="rootValue"></param>
        /// <param name="shrink"></param>
        /// <param name="measure"></param>
        /// <param name="identify"></param>
        /// <returns></returns>
        public static IExampleSpace<T> Unfold<T>(
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

        public static IExampleSpace<T> Delay<T>(
            IExample<T> rootExample,
            Func<IEnumerable<IExampleSpace<T>>> delayedSubspace)
        {
            IEnumerable<U> DelayEnumeration<U>(Func<IEnumerable<U>> source)
            {
                foreach (var element in source())
                {
                    yield return element;
                }
            }

            return new ExampleSpace<T>(rootExample, DelayEnumeration(delayedSubspace));
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

        private static IEnumerable<IExampleSpace<TProjection>> UnfoldSubspaceInternal<TAccumulator, TProjection>(
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

        public static IExampleSpace<TResult> Merge<T, TResult>(
            List<IExampleSpace<T>> exampleSpaces,
            Func<List<T>, TResult> mergeValues,
            ShrinkFunc<List<IExampleSpace<T>>> shrinkExampleSpaces,
            MeasureFunc<List<IExampleSpace<T>>> measureMerge) =>
                MergeInternal(
                    exampleSpaces,
                    mergeValues,
                    shrinkExampleSpaces,
                    measureMerge,
                    ImmutableHashSet.Create<ExampleId>());

        private static IExampleSpace<TResult> MergeInternal<T, TResult>(
            List<IExampleSpace<T>> exampleSpaces,
            Func<List<T>, TResult> mergeValues,
            ShrinkFunc<List<IExampleSpace<T>>> shrinkExampleSpaces,
            MeasureFunc<List<IExampleSpace<T>>> measureMerge,
            ImmutableHashSet<ExampleId> encountered)
        {
            var mergedId = exampleSpaces.Aggregate(
                ExampleId.Primitive(exampleSpaces.Count()),
                (acc, curr) => ExampleId.Combine(acc, curr.Current.Id));

            var mergedValue = mergeValues(exampleSpaces.Select(es => es.Current.Value).ToList());

            var mergedDistance = measureMerge(exampleSpaces);

            var current = new Example<TResult>(
                mergedId,
                mergedValue,
                mergedDistance);

            var exampleSpaceCullingShrinks = shrinkExampleSpaces(exampleSpaces);

            var subspaceMergingShrinks = exampleSpaces
                .Select((exampleSpace, index) => LiftAndInsertSubspace(exampleSpaces, exampleSpace.Subspace, index))
                .SelectMany(exampleSpaces => exampleSpaces);

            var shrinks = TraverseUnencountered(
                Enumerable.Concat(exampleSpaceCullingShrinks, subspaceMergingShrinks),
                (exampleSpaces0, encountered0) =>
                    MergeInternal(exampleSpaces0.ToList(), mergeValues, shrinkExampleSpaces, measureMerge, encountered0),
                encountered);

            return new ExampleSpace<TResult>(current, shrinks);
        }

        private static IEnumerable<IEnumerable<IExampleSpace<T>>> LiftAndInsertSubspace<T>(
            IEnumerable<IExampleSpace<T>> exampleSpaces,
            IEnumerable<IExampleSpace<T>> subspace,
            int index)
        {
            var leftExampleSpaces = exampleSpaces.Take(index);
            var rightExampleSpaces = exampleSpaces.Skip(index + 1);

            return subspace.Select(exampleSpace =>
                Enumerable.Concat(
                    leftExampleSpaces,
                    Enumerable.Concat(new[] { exampleSpace }, rightExampleSpaces)));
        }

        private static IEnumerable<IExampleSpace<T>> TraverseUnencountered<TAccumulator, T>(
            IEnumerable<TAccumulator> accumulators,
            Func<TAccumulator, ImmutableHashSet<ExampleId>, IExampleSpace<T>> generateNextExampleSpace,
            ImmutableHashSet<ExampleId> encountered)
        {
            return accumulators
                .Scan(
                    new UnfoldSubspaceState<IExampleSpace<T>>(
                        new Option.None<IExampleSpace<T>>(),
                        encountered),
                    (acc, curr) =>
                    {
                        var exampleSpace = generateNextExampleSpace(curr, acc.Encountered);

                        var hasBeenEncountered = acc.Encountered.Contains(exampleSpace.Current.Id);
                        if (hasBeenEncountered)
                        {
                            return new UnfoldSubspaceState<IExampleSpace<T>>(
                                new Option.None<IExampleSpace<T>>(),
                                acc.Encountered);
                        }

                        return new UnfoldSubspaceState<IExampleSpace<T>>(
                                new Option.Some<IExampleSpace<T>>(exampleSpace),
                                acc.Encountered.Add(exampleSpace.Current.Id));
                    }
                )
                .Select(x => x.UnencounteredValue switch
                {
                    Option.None<IExampleSpace<T>> _ => null,
                    Option.Some<IExampleSpace<T>> some => some.Value,
                    _ => null
                })
                .Where(exampleSpace => exampleSpace != null)
                .Cast<IExampleSpace<T>>();
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

        public static IExampleSpace<int> Int32(int value, int origin, int min, int max) => Unfold(
            value,
            ShrinkFunc.Towards(origin),
            MeasureFunc.DistanceFromOrigin(origin, min, max),
            IdentifyFuncs.Default<int>());
    }
}
