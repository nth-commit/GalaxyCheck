using System;
using System.Collections.Generic;
using System.Linq;

namespace GalaxyCheck.ExampleSpaces
{
    internal record ExplorationStage<T>
    {
        public record NonCounterexample(IExampleSpace<T> ExampleSpace, IEnumerable<int> Path);

        public record Counterexample(IExampleSpace<T> ExampleSpace, IEnumerable<int> Path, Exception? Exception);

        public record Discard(IExampleSpace<T> ExampleSpace);

        private readonly NonCounterexample? _nonCounterexample;
        private readonly Counterexample? _counterexample;
        private readonly Discard? _discard;

        private ExplorationStage(
            NonCounterexample? nonCounterexample,
            Counterexample? counterexample,
            Discard? discard)
        {
            _nonCounterexample = nonCounterexample;
            _counterexample = counterexample;
            _discard = discard;
        }

        public static class Factory
        {
            public static ExplorationStage<T> NonCounterexample(
                IExampleSpace<T> exampleSpace,
                IEnumerable<int> path) =>
                    new ExplorationStage<T>(
                        nonCounterexample: new NonCounterexample(exampleSpace, path),
                        counterexample: null,
                        discard: null);

            public static ExplorationStage<T> Counterexample(
                IExampleSpace<T> exampleSpace,
                IEnumerable<int> path,
                Exception? exception) =>
                    new ExplorationStage<T>(
                        nonCounterexample: null,
                        counterexample: new Counterexample(exampleSpace, path, exception),
                        discard: null);

            public static ExplorationStage<T> Discard(IExampleSpace<T> exampleSpace) =>
                new ExplorationStage<T>(
                    nonCounterexample: null,
                    counterexample: null,
                    discard: new Discard(exampleSpace));
        }

        public TResult Match<TResult>(
            Func<NonCounterexample, TResult> onNonCounterexampleExploration,
            Func<Counterexample, TResult> onCounterexampleExploration,
            Func<Discard, TResult> onDiscardExploration)
        {
            if (_nonCounterexample != null) return onNonCounterexampleExploration(_nonCounterexample);
            if (_counterexample != null) return onCounterexampleExploration(_counterexample);
            if (_discard != null) return onDiscardExploration(_discard);
            throw new NotSupportedException();
        }
    }

    internal static class ExplorationStageExtensions
    {
        public static bool IsCounterexample<T>(this ExplorationStage<T> explorationStage) =>
            explorationStage.Match(
                onCounterexampleExploration: (_) => true,
                onNonCounterexampleExploration: (_) => false,
                onDiscardExploration: (_) => false);

        public static ExplorationStage<T> PrependPath<T>(
            this ExplorationStage<T> explorationStage,
            IEnumerable<int> path) => explorationStage.Match(

                onNonCounterexampleExploration: nonCounterexample => ExplorationStage<T>.Factory.NonCounterexample(
                    nonCounterexample.ExampleSpace,
                    Enumerable.Concat(path, nonCounterexample.Path)),

                onCounterexampleExploration: counterexample => ExplorationStage<T>.Factory.Counterexample(
                    counterexample.ExampleSpace,
                    Enumerable.Concat(path, counterexample.Path),
                    counterexample.Exception),

                onDiscardExploration: (discard) => ExplorationStage<T>.Factory.Discard(discard.ExampleSpace));

        public static IExampleSpace<T> ExampleSpace<T>(this ExplorationStage<T> explorationStage) =>
            explorationStage.Match(
                onCounterexampleExploration: counterexample => counterexample.ExampleSpace,
                onNonCounterexampleExploration: nonCounterexample => nonCounterexample.ExampleSpace,
                onDiscardExploration: discard => discard.ExampleSpace);

        public static IExample<T> Example<T>(this ExplorationStage<T> explorationStage) =>
            explorationStage.ExampleSpace().Current;

        public static T Value<T>(this ExplorationStage<T> explorationStage) =>
            explorationStage.Example().Value;
    }
}
