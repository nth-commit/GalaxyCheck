using GalaxyCheck;
using GalaxyCheck.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GalaxyCheck.ExampleSpaces
{
    public static class ExampleSpaceExtensions
    {
        public static IExampleSpace<T> Cast<T>(this IExampleSpace exampleSpace)
        {
            var example = new Example<T>(
                exampleSpace.Current.Id,
                (T)exampleSpace.Current.Value,
                exampleSpace.Current.Distance);

            var subspace = exampleSpace.Subspace.Select(child => Cast<T>(child));

            return ExampleSpaceFactory.Create(example, subspace);
        }

        /// <summary>
        /// Maps all of the example values in an example space by the given selector function.
        /// </summary>
        /// <typeparam name="T">The type of the target example space's values.</typeparam>
        /// <typeparam name="TResult">The new type of an example's value</typeparam>
        /// <param name="exampleSpace">The example space to operate on.</param>
        /// <param name="selector">A function to apply to each value in the example space.</param>
        /// <returns>A new example space with the mapping function applied.</returns>
        public static IExampleSpace<TResult> Map<T, TResult>(
            this IExampleSpace<T> exampleSpace,
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
        public static IExampleSpace<TResult> MapExamples<T, TResult>(
            this IExampleSpace<T> exampleSpace,
            Func<IExample<T>, IExample<TResult>> f)
        {
            return ExampleSpaceFactory.Create(
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
        public static IExampleSpace<T>? Filter<T>(
            this IExampleSpace<T> exampleSpace,
            Func<T, bool> pred)
        {
            if (pred(exampleSpace.Current.Value) == false) return null;

            return ExampleSpaceFactory.Create(
                exampleSpace.Current,
                exampleSpace.Subspace
                    .Select(es => es.Filter(pred))
                    .Where(es => es != null)
                    .Cast<IExampleSpace<T>>());
        }

        private record CounterexampleSubspace<T>(
            IExampleSpace<T> CounterexampleSpace,
            IEnumerable<int> Path);

        private record SubspaceExploration<T>(
            CounterexampleSubspace<T>? CounterexampleSubspace,
            IEnumerable<ExplorationStage<T>> ExplorationStages);

        public static IEnumerable<ExplorationStage<T>> Explore<T>(
            this IExampleSpace<T> exampleSpace,
            AnalyzeExploration<T> analyze)
        {
            static ExplorationStage<T> AnalyzeExampleSpace(
                IExampleSpace<T> exampleSpace,
                IEnumerable<int> path,
                AnalyzeExploration<T> analyze)
            {
                return analyze(exampleSpace.Current).Match(
                    onSuccess: () => ExplorationStage<T>.Factory.NonCounterexample(exampleSpace, path),
                    onDiscard: () => ExplorationStage<T>.Factory.Discard(exampleSpace),
                    onFail: exception => ExplorationStage<T>.Factory.Counterexample(exampleSpace, path, exception));
            }

            static SubspaceExploration<T> GetSubspaceExploration(AnalyzeExploration<T> analyze, IExampleSpace<T> exampleSpace) =>
                exampleSpace.Subspace
                    .Select((exampleSpace, subspaceIndex) =>
                    {
                        var path = new[] { subspaceIndex };
                        var explorationStage = AnalyzeExampleSpace(exampleSpace, path, analyze);
                        return (explorationStage, exampleSpace);
                    })
                    .TakeWhileInclusive(x => !x.explorationStage.IsCounterexample())
                    .Aggregate(
                        new SubspaceExploration<T>(null, Enumerable.Empty<ExplorationStage<T>>()),
                        (acc, curr) =>
                        {
                            var maybeCounterexampleSubspace = curr.explorationStage.Match<CounterexampleSubspace<T>?>(
                                onCounterexampleExploration: (counterexample) => new CounterexampleSubspace<T>(counterexample.ExampleSpace, counterexample.Path),
                                onNonCounterexampleExploration: _ => null,
                                onDiscardExploration: _ => null);

                            return new SubspaceExploration<T>(
                                maybeCounterexampleSubspace,
                                Enumerable.Concat(acc.ExplorationStages, new[] { curr.explorationStage }));
                        });

            static IEnumerable<ExplorationStage<T>> ExploreSubspace(AnalyzeExploration<T> analyze, IExampleSpace<T> exampleSpace)
            {
                CounterexampleSubspace<T>? counterexampleSubspace = new CounterexampleSubspace<T>(exampleSpace, Enumerable.Empty<int>());

                while (counterexampleSubspace != null)
                {
                    var subspaceExploration = GetSubspaceExploration(analyze, counterexampleSubspace.CounterexampleSpace);
                    var previousPath = counterexampleSubspace.Path;

                    counterexampleSubspace = null;

                    foreach (var explorationStage in subspaceExploration.ExplorationStages.Select(es => es.PrependPath(previousPath)))
                    {
                        yield return explorationStage;

                        counterexampleSubspace = explorationStage.Match<CounterexampleSubspace<T>?>(
                            onCounterexampleExploration: counterexampleExploration =>
                                new CounterexampleSubspace<T>(counterexampleExploration.ExampleSpace, counterexampleExploration.Path),
                            onNonCounterexampleExploration: _ => null,
                            onDiscardExploration: _ => null);
                    }
                }
            }

            /// <summary>
            /// Crashes on MacOS, stackoverflahhhh :'(
            /// 
            /// I wanna leave this as an easily accessible example for me (and I'm too lazy to establish an index system).
            /// I'm really bad at doing non-recursive solutions to obviously recursive problems.
            /// </summary>
            static IEnumerable<ExplorationStage<T>> ExploreSubspaceRec(AnalyzeExploration<T> analyze, IExampleSpace<T> exampleSpace)
            {
                var subspaceExploration = GetSubspaceExploration(analyze, exampleSpace);

                foreach (var explorationStage in subspaceExploration.ExplorationStages)
                {
                    yield return explorationStage;
                }

                var counterexampleSubspace = subspaceExploration.CounterexampleSubspace;
                if (counterexampleSubspace != null)
                {
                    foreach (var childExplorationStage in ExploreSubspaceRec(analyze, counterexampleSubspace.CounterexampleSpace))
                    {
                        yield return childExplorationStage.PrependPath(counterexampleSubspace.Path);
                    }
                }
            }

            var rootExplorationStage = AnalyzeExampleSpace(
                exampleSpace,
                Enumerable.Empty<int>(),
                analyze);

            return rootExplorationStage.IsCounterexample()
                ? Enumerable.Concat(new[] { rootExplorationStage }, ExploreSubspace(analyze, exampleSpace))
                : new[] { rootExplorationStage };
        }

        public static IExampleSpace<T>? Navigate<T>(this IExampleSpace<T> exampleSpace, IEnumerable<int> path)
        {
            static IExampleSpace<T>? NavigateRec(IExampleSpace<T> exampleSpace, IEnumerable<int> path)
            {
                if (path.Any() == false) return exampleSpace;

                var nextExampleSpace = exampleSpace.Subspace.Skip(path.First()).FirstOrDefault();

                if (nextExampleSpace == null) return null;

                return NavigateRec(nextExampleSpace, path.Skip(1).ToList());
            }

            return NavigateRec(exampleSpace, path);
        }

        public static IEnumerable<ExplorationStage<T>> ExploreCounterexamples<T>(
            this IExampleSpace<T> exampleSpace,
            Func<T, bool> pred)
        {
            AnalyzeExploration<T> analyze = (explorationStage) => pred(explorationStage.Value)
                ? ExplorationOutcome.Success()
                : ExplorationOutcome.Fail(null);

            return exampleSpace.Explore(analyze);
        }
    }
}
