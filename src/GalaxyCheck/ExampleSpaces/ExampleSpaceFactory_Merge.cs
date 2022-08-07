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
                IExampleSpace<IReadOnlyCollection<T>> GenerateNextExampleSpace(IEnumerable<IExampleSpace<T>> exampleSpaces, ImmutableHashSet<ExampleId> encounteredIds)
                {
                    return MergeInternal(
                        exampleSpaces.ToList(),
                        shrinkExampleSpaces,
                        measureMerge,
                        encounteredIds,
                        enableSmallestExampleSpacesOptimization,
                        false);
                }

                var mergedId = exampleSpaces.Aggregate(
                    ExampleId.Primitive(exampleSpaces.Count()),
                    (acc, curr) => ExampleId.Combine(acc, curr.Current.Id));

                var mergedDistance = measureMerge(exampleSpaces);

                var current = new Example<IReadOnlyCollection<T>>(
                    mergedId,
                    exampleSpaces.Select(es => es.Current.Value).ToList(),
                    mergedDistance);

                var smallestExampleSpacesShrink = enableSmallestExampleSpacesOptimization && isRoot && exampleSpaces.Any(exampleSpace => exampleSpace.Subspace.Any())
                    ? exampleSpaces
                        .Select(exampleSpace =>
                        {
                            var smallestExampleSpace = exampleSpace.Subspace.FirstOrDefault() ?? exampleSpace;
                            return smallestExampleSpace;
                        })
                        .ToList()
                    : null;

                var exampleSpaceCullingShrinks = shrinkExampleSpaces(exampleSpaces);

                var subspaceMergingShrinks = exampleSpaces
                    .Select((exampleSpace, index) => LiftAndInsertSubspace(exampleSpaces, exampleSpace.Subspace, index))
                    .SelectMany(exampleSpaces => exampleSpaces);

                var shrinks = TraverseUnencountered(
                    Enumerable.Concat(
                        smallestExampleSpacesShrink == null ? Enumerable.Empty<List<IExampleSpace<T>>>() : new[] { smallestExampleSpacesShrink }.AsEnumerable(),
                        Enumerable.Concat(exampleSpaceCullingShrinks, subspaceMergingShrinks)),
                    encounteredIds,
                    GenerateNextExampleSpace);

                return new ExampleSpace<IReadOnlyCollection<T>>(current, shrinks);
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
