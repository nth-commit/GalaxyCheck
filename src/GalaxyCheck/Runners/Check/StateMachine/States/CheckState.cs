using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GalaxyCheck.ExampleSpaces;
using GalaxyCheck.Gens.Parameters;

namespace GalaxyCheck.Runners.Check.StateMachine.States
{
    internal interface CheckState<Test>
    {
    }

    internal interface TransitionableCheckState<Test> : CheckState<Test>
    {
        CheckStateTransition<Test> Transition(CheckStateData<Test> checkStateData);
    }

    internal interface InterruptedCheckState<Test> : CheckState<Test>
    {
    }

    internal record CheckStateData<Test>(
        IGen<Test> Property,
        ResizeStrategy<Test> ResizeStrategy,
        int CompletedIterationsUntilCounterexample,
        int CompletedIterationsAfterCounterexample,
        int RequestedIterations,
        GenParameters NextParameters,
        int Discards,
        int ConsecutiveDiscards,
        int ShrinkLimit,
        int TotalShrinks,
        bool DeepCheck,
        bool IsReplay,
        ImmutableList<CheckStateData<Test>.CounterexampleData> Counterexamples)
    {
        public int CompletedIterations =>
            CompletedIterationsUntilCounterexample + CompletedIterationsAfterCounterexample;

        public CounterexampleData? Counterexample => Counterexamples
            .OrderBy(c => c.ExampleSpace.Current.Distance)
            // Bigger sizes are more likely to normalize, because they have more shrinking permutations to explore
            .ThenByDescending(c => c.ReplayParameters.Size.Value)
            .FirstOrDefault();

        public CheckStateData<Test> IncrementCompletedIterations()
        {
            var hasCounterexample = Counterexamples.Any();
            return hasCounterexample
                ? this with
                {
                    CompletedIterationsAfterCounterexample = CompletedIterationsAfterCounterexample + 1,
                    ConsecutiveDiscards = 0
                }
                : this with
                {
                    CompletedIterationsUntilCounterexample = CompletedIterationsUntilCounterexample + 1,
                    ConsecutiveDiscards = 0
                };
        }

        public CheckStateData<Test> IncrementDiscards()
        {
            var discards = Discards + 1;
            var consecutiveDiscards = ConsecutiveDiscards + 1;

            if (consecutiveDiscards > 100)
            {
                // Bit of a hack to do this here
                throw new Exceptions.GenExhaustionException();
            }

            return this with
            {
                Discards = discards,
                ConsecutiveDiscards = consecutiveDiscards
            };
        }

        public CheckStateData<Test> IncrementShrinks() => this with
        {
            TotalShrinks = TotalShrinks + 1
        };

        public CheckStateData<Test> WithNextGenParameters(GenParameters nextParameters) => this with
        {
            NextParameters = nextParameters
        };

        public static CheckStateData<Test> Initial(
            IGen<Test> property,
            ResizeStrategy<Test> resizeStrategy,
            int requestedIterations,
            int shrinkLimit,
            bool deepCheck,
            bool isReplay,
            GenParameters initialParameters) =>
            new(
                Property: property,
                ResizeStrategy: resizeStrategy,
                CompletedIterationsUntilCounterexample: 0,
                CompletedIterationsAfterCounterexample: 0,
                RequestedIterations: requestedIterations,
                NextParameters: initialParameters,
                Discards: 0,
                ConsecutiveDiscards: 0,
                ShrinkLimit: shrinkLimit,
                TotalShrinks: 0,
                DeepCheck: deepCheck,
                IsReplay: isReplay,
                Counterexamples: ImmutableList<CounterexampleData>.Empty
            );

        public CheckStateData<Test> AddCounterexample(
            ExplorationStage<Test>.Counterexample counterexample,
            GenParameters instanceReplayParameters
        ) => this with
        {
            Counterexamples = Counterexamples.Add(new CounterexampleData(
                counterexample.ExampleSpace,
                counterexample.Path,
                counterexample.Exception,
                instanceReplayParameters))
        };

        public record CounterexampleData(
            IExampleSpace<Test> ExampleSpace,
            IEnumerable<int> Path,
            Exception? Exception,
            GenParameters ReplayParameters
        ) : ExplorationStage<Test>.Counterexample(ExampleSpace, Path, Exception);
    }

    internal record CheckStateTransition<Test>(
        CheckState<Test> NextState,
        CheckStateData<Test> NextStateData);
}
