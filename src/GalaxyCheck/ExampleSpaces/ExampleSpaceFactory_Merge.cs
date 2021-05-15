using GalaxyCheck.Internal;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GalaxyCheck.ExampleSpaces
{
    internal static partial class ExampleSpaceFactory
    {
        public static IExampleSpace<TResult> Merge<T, TResult>(
            List<IExampleSpace<T>> exampleSpaces,
            Func<List<T>, TResult> mergeValues,
            ShrinkFunc<List<IExampleSpace<T>>> shrinkExampleSpaces,
            MeasureFunc<List<IExampleSpace<T>>> measureMerge) =>
                MergeHelpers.MergeInternal(
                    exampleSpaces,
                    mergeValues,
                    shrinkExampleSpaces,
                    measureMerge,
                    ImmutableHashSet.Create<ExampleId>());

        private static class MergeHelpers
        {
            public static IExampleSpace<TResult> MergeInternal<T, TResult>(
                List<IExampleSpace<T>> exampleSpaces,
                Func<List<T>, TResult> mergeValues,
                ShrinkFunc<List<IExampleSpace<T>>> shrinkExampleSpaces,
                MeasureFunc<List<IExampleSpace<T>>> measureMerge,
                ImmutableHashSet<ExampleId> encounteredIds)
            {
                IExampleSpace<TResult> GenerateNextExampleSpace(IEnumerable<IExampleSpace<T>> exampleSpaces, ImmutableHashSet<ExampleId> encounteredIds)
                {
                    return MergeInternal(exampleSpaces.ToList(), mergeValues, shrinkExampleSpaces, measureMerge, encounteredIds);
                }

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
                    encounteredIds,
                    GenerateNextExampleSpace);

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
                ImmutableHashSet<ExampleId> encounteredIds,
                Func<TAccumulator, ImmutableHashSet<ExampleId>, IExampleSpace<T>> nextExampleSpace)
            {
                return accumulators
                    .Scan(
                        new UnfoldSubspaceState<T>(
                            new Option.None<IExampleSpace<T>>(),
                            encounteredIds),
                        (acc, curr) =>
                        {
                            var exampleSpace = nextExampleSpace(curr, acc.Encountered);

                            var hasBeenEncountered = acc.Encountered.Contains(exampleSpace.Current.Id);
                            if (hasBeenEncountered)
                            {
                                return new UnfoldSubspaceState<T>(
                                    new Option.None<IExampleSpace<T>>(),
                                    acc.Encountered);
                            }

                            return new UnfoldSubspaceState<T>(
                                new Option.Some<IExampleSpace<T>>(exampleSpace),
                                acc.Encountered.Add(exampleSpace.Current.Id));
                        }
                    )
                    .Select(x => x.UnencounteredExampleSpace switch
                    {
                        Option.Some<IExampleSpace<T>> some => some.Value,
                        _ => null
                    })
                    .Where(exampleSpace => exampleSpace != null)
                    .Cast<IExampleSpace<T>>();
            }

            private class UnfoldSubspaceState<T>
            {
                public Option<IExampleSpace<T>> UnencounteredExampleSpace { get; init; }

                public ImmutableHashSet<ExampleId> Encountered { get; init; }

                public UnfoldSubspaceState(
                    Option<IExampleSpace<T>> unencounteredExampleSpace,
                    ImmutableHashSet<ExampleId> encountered)
                {
                    UnencounteredExampleSpace = unencounteredExampleSpace;
                    Encountered = encountered;
                }
            }
        }
    }
}
