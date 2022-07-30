using GalaxyCheck.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GalaxyCheck.ExampleSpaces
{
    internal static class ExampleSpaceExtensions
    {
        public static IExampleSpace<T> Cast<T>(this IExampleSpace exampleSpace)
        {
            T newValue;
            try
            {
                newValue = (T)exampleSpace.Current.Value;
            }
            catch
            {
                newValue = (T)Convert.ChangeType(exampleSpace.Current.Value, typeof(T));
            }

            var example = new Example<T>(exampleSpace.Current.Id, newValue, exampleSpace.Current.Distance);

            var subspace = exampleSpace.Subspace.Select(child => Cast<T>(child));

            return ExampleSpaceFactory.Create(example, subspace);
        }

        /// <summary>
        /// Maps all of the example values in an example space by the given selector function.
        /// </summary>
        /// <typeparam name="T">The type of the target example space's values.</typeparam>
        /// <typeparam name="TResult">The new type of an example's value</typeparam>
        /// <param name="exampleSpace">The example space to operate on.</param>
        /// <param name="f">A function to apply to each value in the example space.</param>
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
        /// <param name="f">A function to apply to each example in the example space.</param>
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
                    onFail: exception => ExplorationStage<T>.Factory.Counterexample(exampleSpace, path, exception));
            }

            static IEnumerable<ExplorationStage<T>> GetSubspaceExploration(AnalyzeExploration<T> analyze, IExampleSpace<T> exampleSpace) =>
                exampleSpace.Subspace
                    .Select((exampleSpace, subspaceIndex) =>
                    {
                        var path = new[] { subspaceIndex };
                        var explorationStage = AnalyzeExampleSpace(exampleSpace, path, analyze);
                        return explorationStage;
                    })
                    .TakeWhileInclusive(explorationStage => explorationStage.IsCounterexample() == false);

            static IEnumerable<ExplorationStage<T>> ExploreSubspace(AnalyzeExploration<T> analyze, IExampleSpace<T> exampleSpace)
            {
                CounterexampleSubspace<T>? counterexampleSubspace = new CounterexampleSubspace<T>(exampleSpace, Enumerable.Empty<int>());

                while (counterexampleSubspace != null)
                {
                    var subspaceExploration = GetSubspaceExploration(analyze, counterexampleSubspace.CounterexampleSpace);
                    var previousPath = counterexampleSubspace.Path;

                    counterexampleSubspace = null;

                    foreach (var explorationStage in subspaceExploration.Select(es => es.PrependPath(previousPath)))
                    {
                        yield return explorationStage;

                        counterexampleSubspace = explorationStage.Match<CounterexampleSubspace<T>?>(
                            onCounterexampleExploration: counterexampleExploration =>
                                new CounterexampleSubspace<T>(counterexampleExploration.ExampleSpace, counterexampleExploration.Path),
                            onNonCounterexampleExploration: _ => null);
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

        public static async IAsyncEnumerable<ExplorationStage<T>> ExploreAsync<T>(
            this IExampleSpace<T> exampleSpace,
            AnalyzeExplorationAsync<T> analyze)
        {
            static async Task<ExplorationStage<T>> AnalyzeExampleSpace(
                IExampleSpace<T> exampleSpace,
                IEnumerable<int> path,
                AnalyzeExplorationAsync<T> analyze)
            {
                return (await analyze(exampleSpace.Current)).Match(
                    onSuccess: () => ExplorationStage<T>.Factory.NonCounterexample(exampleSpace, path),
                    onFail: exception => ExplorationStage<T>.Factory.Counterexample(exampleSpace, path, exception));
            }

            static IAsyncEnumerable<ExplorationStage<T>> GetSubspaceExploration(AnalyzeExplorationAsync<T> analyze, IExampleSpace<T> exampleSpace) =>
                exampleSpace.Subspace
                    .ToAsyncEnumerable()
                    .SelectAwait(async (exampleSpace, subspaceIndex) =>
                    {
                        var path = new[] { subspaceIndex };
                        return await AnalyzeExampleSpace(exampleSpace, path, analyze);
                    })
                    .TakeWhileInclusive(explorationStage => explorationStage.IsCounterexample() == false);

            static async IAsyncEnumerable<ExplorationStage<T>> ExploreSubspace(AnalyzeExplorationAsync<T> analyze, IExampleSpace<T> exampleSpace)
            {
                CounterexampleSubspace<T>? counterexampleSubspace = new CounterexampleSubspace<T>(exampleSpace, Enumerable.Empty<int>());

                while (counterexampleSubspace != null)
                {
                    var subspaceExploration = GetSubspaceExploration(analyze, counterexampleSubspace.CounterexampleSpace);
                    var previousPath = counterexampleSubspace.Path;

                    counterexampleSubspace = null;

                    await foreach (var explorationStage in subspaceExploration.Select(es => es.PrependPath(previousPath)))
                    {
                        yield return explorationStage;

                        counterexampleSubspace = explorationStage.Match<CounterexampleSubspace<T>?>(
                            onCounterexampleExploration: counterexampleExploration =>
                                new CounterexampleSubspace<T>(counterexampleExploration.ExampleSpace, counterexampleExploration.Path),
                            onNonCounterexampleExploration: _ => null);
                    }
                }
            }
            
            var rootExplorationStage = await AnalyzeExampleSpace(
                exampleSpace,
                Enumerable.Empty<int>(),
                analyze);

            yield return rootExplorationStage;

            if (rootExplorationStage.IsCounterexample() == false) yield break;
            
            await foreach (var explorationStage in ExploreSubspace(analyze, exampleSpace))
            {
                yield return explorationStage;
            }
        }
    }
}
