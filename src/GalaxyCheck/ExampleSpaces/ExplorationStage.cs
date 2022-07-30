using System;
using System.Collections.Generic;
using System.Linq;

namespace GalaxyCheck.ExampleSpaces
{
    internal record ExplorationStage<T>
    {
        public record NonCounterexample(IExampleSpace<T> ExampleSpace, IEnumerable<int> Path);

        public record Counterexample(IExampleSpace<T> ExampleSpace, IEnumerable<int> Path, Exception? Exception);

        private readonly NonCounterexample? _nonCounterexample;
        private readonly Counterexample? _counterexample;

        private ExplorationStage(
            NonCounterexample? nonCounterexample,
            Counterexample? counterexample)
        {
            _nonCounterexample = nonCounterexample;
            _counterexample = counterexample;
        }

        public static class Factory
        {
            public static ExplorationStage<T> NonCounterexample(
                IExampleSpace<T> exampleSpace,
                IEnumerable<int> path) =>
                    new ExplorationStage<T>(
                        nonCounterexample: new NonCounterexample(exampleSpace, path),
                        counterexample: null);

            public static ExplorationStage<T> Counterexample(
                IExampleSpace<T> exampleSpace,
                IEnumerable<int> path,
                Exception? exception) =>
                    new ExplorationStage<T>(
                        nonCounterexample: null,
                        counterexample: new Counterexample(exampleSpace, path, exception));
        }

        public TResult Match<TResult>(
            Func<NonCounterexample, TResult> onNonCounterexampleExploration,
            Func<Counterexample, TResult> onCounterexampleExploration)
        {
            if (_nonCounterexample != null) return onNonCounterexampleExploration(_nonCounterexample);
            if (_counterexample != null) return onCounterexampleExploration(_counterexample);
            throw new NotSupportedException();
        }
    }

    internal static class ExplorationStageExtensions
    {
        public static bool IsCounterexample<T>(this ExplorationStage<T> explorationStage) =>
            explorationStage.Match(
                onCounterexampleExploration: (_) => true,
                onNonCounterexampleExploration: (_) => false);

        public static ExplorationStage<T> PrependPath<T>(
            this ExplorationStage<T> explorationStage,
            IEnumerable<int> path) => explorationStage.Match(

                onNonCounterexampleExploration: nonCounterexample => ExplorationStage<T>.Factory.NonCounterexample(
                    nonCounterexample.ExampleSpace,
                    Enumerable.Concat(path, nonCounterexample.Path)),

                onCounterexampleExploration: counterexample => ExplorationStage<T>.Factory.Counterexample(
                    counterexample.ExampleSpace,
                    Enumerable.Concat(path, counterexample.Path),
                    counterexample.Exception));

        public static IExampleSpace<T> ExampleSpace<T>(this ExplorationStage<T> explorationStage) =>
            explorationStage.Match(
                onCounterexampleExploration: counterexample => counterexample.ExampleSpace,
                onNonCounterexampleExploration: nonCounterexample => nonCounterexample.ExampleSpace);

        public static IExample<T> Example<T>(this ExplorationStage<T> explorationStage) =>
            explorationStage.ExampleSpace().Current;

        public static T Value<T>(this ExplorationStage<T> explorationStage) =>
            explorationStage.Example().Value;
    }
}
