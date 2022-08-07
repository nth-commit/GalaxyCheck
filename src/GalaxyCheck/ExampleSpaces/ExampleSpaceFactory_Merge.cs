using GalaxyCheck.Internal;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GalaxyCheck.ExampleSpaces
{
    internal static partial class ExampleSpaceFactory
    {
        public static IExampleSpace<IReadOnlyCollection<T>> Merge<T>(
            IReadOnlyCollection<IExampleSpace<T>> exampleSpaces,
            ShrinkFunc<IReadOnlyCollection<IExampleSpace<T>>> shrinkExampleSpaces,
            MeasureFunc<IReadOnlyCollection<IExampleSpace<T>>> measureMerge,
            bool enableSmallestExampleSpacesOptimization) =>
                MergeHelpers.MergeInternal(
                    exampleSpaces,
                    shrinkExampleSpaces,
                    measureMerge,
                    ImmutableHashSet.Create<ExampleId>(),
                    enableSmallestExampleSpacesOptimization,
                    true);

        private static class MergeHelpers
        {
            public static IExampleSpace<IReadOnlyCollection<T>> MergeInternal<T>(
                IReadOnlyCollection<IExampleSpace<T>> exampleSpaces,
                ShrinkFunc<IReadOnlyCollection<IExampleSpace<T>>> shrinkExampleSpaces,
                MeasureFunc<IReadOnlyCollection<IExampleSpace<T>>> measureMerge,
                ImmutableHashSet<ExampleId> encounteredIds,
                bool enableSmallestExampleSpacesOptimization,
                bool isRoot)
            {
                IExampleSpace<IReadOnlyCollection<T>> GenerateNextExampleSpace(IEnumerable<IExampleSpace<T>> exampleSpaces, ImmutableHashSet<ExampleId> encounteredIds) => MergeInternal(
                    exampleSpaces.ToList(),
                    shrinkExampleSpaces,
                    measureMerge,
                    encounteredIds,
                    enableSmallestExampleSpacesOptimization,
                    false);

                var mergedId = exampleSpaces.Aggregate(
                    ExampleId.Primitive(exampleSpaces.Count()),
                    (acc, curr) => ExampleId.Combine(acc, curr.Current.Id));

                var mergedDistance = measureMerge(exampleSpaces);

                var current = new Example<IReadOnlyCollection<T>>(
                    mergedId,
                    exampleSpaces.Select(es => es.Current.Value).ToList(),
                    mergedDistance);

                var shrinks = Shrink(exampleSpaces, shrinkExampleSpaces, encounteredIds, enableSmallestExampleSpacesOptimization, isRoot, GenerateNextExampleSpace);

                return new ExampleSpace<IReadOnlyCollection<T>>(current, shrinks);
            }

            private static IEnumerable<IExampleSpace<IReadOnlyCollection<T>>> Shrink<T>(
                IReadOnlyCollection<IExampleSpace<T>> exampleSpaces,
                ShrinkFunc<IReadOnlyCollection<IExampleSpace<T>>> shrinkExampleSpaces,
                ImmutableHashSet<ExampleId> encounteredIds,
                bool enableSmallestExampleSpacesOptimization,
                bool isRoot,
                Func<IEnumerable<IExampleSpace<T>>, ImmutableHashSet<ExampleId>, IExampleSpace<IReadOnlyCollection<T>>> nextExampleSpace)
            {
                var preMergeShrinks = PreMergeShrink(exampleSpaces, shrinkExampleSpaces, enableSmallestExampleSpacesOptimization, isRoot);

                var shrinks = TraverseUnencountered(
                    preMergeShrinks,
                    encounteredIds,
                    nextExampleSpace);

                foreach (var shrink in shrinks)
                {
                    yield return shrink;
                }
            }

            private static IEnumerable<IEnumerable<IExampleSpace<T>>> PreMergeShrink<T>(
                IReadOnlyCollection<IExampleSpace<T>> exampleSpaces,
                ShrinkFunc<IReadOnlyCollection<IExampleSpace<T>>> shrinkExampleSpaces,
                bool enableSmallestExampleSpacesOptimization,
                bool isRoot)
            {
                if (enableSmallestExampleSpacesOptimization && isRoot && exampleSpaces.Any(exampleSpace => exampleSpace.Subspace.Any()))
                {
                    var smallestExampleSpacesShrink = exampleSpaces
                        .Select(exampleSpace =>
                        {
                            var smallestExampleSpace = exampleSpace.Subspace.FirstOrDefault() ?? exampleSpace;
                            return smallestExampleSpace;
                        })
                        .ToList();

                    yield return smallestExampleSpacesShrink;
                }

                var exampleSpaceCullingShrinks = shrinkExampleSpaces(exampleSpaces);
                foreach (var shrink in exampleSpaceCullingShrinks)
                {
                    yield return shrink;
                }

                var subspaceMergingShrinks = exampleSpaces
                    .Select((exampleSpace, index) => LiftAndInsertSubspace(exampleSpaces, exampleSpace.Subspace, index))
                    .SelectMany(exampleSpaces => exampleSpaces);
                foreach (var shrink in subspaceMergingShrinks)
                {
                    yield return shrink;
                }
            }

            private static IEnumerable<IEnumerable<IExampleSpace<T>>> LiftAndInsertSubspace<T>(
                IEnumerable<IExampleSpace<T>> exampleSpaces,
                IEnumerable<IExampleSpace<T>> subspace,
                int index)
            {
                // TODO: Try fix multiple enumeration here
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
                    .Where(exampleSpace => exampleSpace != null)!;
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
