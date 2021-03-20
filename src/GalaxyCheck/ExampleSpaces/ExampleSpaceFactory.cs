using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GalaxyCheck.ExampleSpaces
{
    public static partial class ExampleSpaceFactory
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
            IExampleSpace<TResult> GenerateNextExampleSpace(
                IEnumerable<IExampleSpace<T>> exampleSpaces,
                ImmutableHashSet<ExampleId> encountered)
            {
                return MergeInternal(exampleSpaces.ToList(), mergeValues, shrinkExampleSpaces, measureMerge, encountered);
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
                encountered,
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

        public static IExampleSpace<int> Int32(int value, int origin, int min, int max) => Unfold(
            value,
            ShrinkFunc.Towards(origin),
            MeasureFunc.DistanceFromOrigin(origin, min, max),
            IdentifyFuncs.Default<int>());

        public static IExampleSpace<int> Int32Optimized(int value, int origin, int min, int max) => Unfold(
            value,
            new Int32OptimizedContextualShrinker(origin),
            MeasureFunc.DistanceFromOrigin(origin, min, max),
            IdentifyFuncs.Default<int>());

        private record Int32OptimizedContext(
            ImmutableStack<int> EncounteredValues,
            bool IsRoot);

        private class Int32OptimizedContextualShrinker : ContextualShrinker<int, Int32OptimizedContext>
        {
            private readonly int _origin;

            public Int32OptimizedContextualShrinker(int origin)
            {
                _origin = origin;
            }

            public Int32OptimizedContext RootContext => new Int32OptimizedContext(ImmutableStack.Create<int>(), IsRoot: true);

            public ContextualShrinkFunc<int, Int32OptimizedContext> ContextualShrink => (value, context) =>
            {
                var nextContext = new Int32OptimizedContext(ImmutableStack.Create<int>(), IsRoot: false);
                var shrinks = ShrinkInt32Optimized(value, _origin, context);
                return (nextContext, shrinks);
            };

            public NextContextFunc<int, Int32OptimizedContext> ContextualTraverse => (value, context) =>
            {
                return new Int32OptimizedContext(context.EncounteredValues.Push(value), IsRoot: context.IsRoot);
            };

            private static IEnumerable<int> ShrinkInt32Optimized(int value, int origin, Int32OptimizedContext context)
            {
                if (context.IsRoot)
                {
                    // Just shrink to the origin, regardless
                    return ShrinkFunc.Towards(origin)(value);
                }

                var previousEncounteredValues = context.EncounteredValues.Skip(1);
                if (previousEncounteredValues.Any() == false)
                {
                    // There are no shrinks left
                    return Enumerable.Empty<int>();
                }

                var difference = value >= origin ? 1 : -1;
                return ShrinkFunc.Towards(previousEncounteredValues.First() + difference)(value);
            }
        }
    }
}
