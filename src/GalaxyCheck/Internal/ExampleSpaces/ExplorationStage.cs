using System;
using System.Collections.Generic;

namespace GalaxyCheck.Internal.ExampleSpaces
{
    public record ExplorationStage<T>
    {
        public record NonCounterexample(IExampleSpace<T> ExampleSpace, IEnumerable<int> Path);

        public record Counterexample(IExampleSpace<T> ExampleSpace, IEnumerable<int> Path, Exception? Exception);

        public record Discard();

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
                IEnumerable<int> path) => new ExplorationStage<T>(
                    nonCounterexample: new NonCounterexample(exampleSpace, path),
                    counterexample: null,
                    discard: null);

            public static ExplorationStage<T> Counterexample(
                IExampleSpace<T> exampleSpace,
                IEnumerable<int> path,
                Exception? exception) => new ExplorationStage<T>(
                    nonCounterexample: null,
                    counterexample: new Counterexample(exampleSpace, path, exception),
                    discard: null);

            public static ExplorationStage<T> Discard() => new ExplorationStage<T>(
                nonCounterexample: null,
                counterexample: null,
                discard: new Discard());
        }

        public TResult Match<TResult>(
            Func<NonCounterexample, TResult> onNonCounterexampleExploration,
            Func<Counterexample, TResult> onCounterexampleExploration,
            Func<TResult> onDiscardExploration)
        {
            if (_nonCounterexample != null) return onNonCounterexampleExploration(_nonCounterexample);
            if (_counterexample != null) return onCounterexampleExploration(_counterexample);
            if (_discard != null) return onDiscardExploration();
            throw new NotSupportedException();
        }
    }

    public static class ExplorationStage2Extensions
    {
        public static bool IsCounterexample<T>(this ExplorationStage<T> explorationStage) =>
            explorationStage.Match(
                onCounterexampleExploration: (_) => true,
                onNonCounterexampleExploration: (_) => false,
                onDiscardExploration: () => false);
    }
}
