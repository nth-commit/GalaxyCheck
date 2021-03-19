using GalaxyCheck.Gens.Iterations.Generic;
using GalaxyCheck.Internal.ExampleSpaces;
using GalaxyCheck.Internal.Replaying;
using System;
using System.Linq;
using static GalaxyCheck.CheckExtensions;

namespace GalaxyCheck.Runners.CheckAutomata
{
    public record ReplayTransition<T>(
        CheckState<T> State,
        string ReplayEncoded) : AbstractTransition<T>(State)
    {
        internal override AbstractTransition<T> NextTransition()
        {
            Replay replayDecoded;
            try
            {
                replayDecoded = ReplayEncoding.Decode(ReplayEncoded);
            }
            catch (Exception ex)
            {
                return new ErrorTransition<T>(State, $"Error decoding replay string: {ex.Message}");
            }

            var iteration = State.Property.Advanced.Run(replayDecoded.GenParameters).First();

            return iteration.Match(
                onInstance: instance => HandleInstance(State, instance, replayDecoded),
                onError: error => HandleError(State),
                onDiscard: _ => HandleError(State));
        }

        private static AbstractTransition<T> HandleInstance(CheckState<T> state, IGenInstance<Test<T>> instance, Replay replayDecoded)
        {
            var exampleSpace = instance.ExampleSpace.Navigate(replayDecoded.ExampleSpacePath);
            if (exampleSpace == null)
            {
                return HandleError(state);
            }

            var exploration = exampleSpace.Explore(AnalyzeExplorationForCheck.Impl<T>()).First();

            return exploration.Match(
                onNonCounterexampleExploration: _ => HandleReplayedNonCounterexample(state, instance),
                onCounterexampleExploration: counterexample => HandleReplayedCounterexample(state, instance, replayDecoded, counterexample),
                onDiscardExploration: () => HandleError(state));
        }

        private static AbstractTransition<T> HandleReplayedNonCounterexample(
            CheckState<T> state,
            IGenInstance<Test<T>> instance)
        {
            return new InstanceCompleteTransition<T>(
                state,
                instance,
                null,
                false,
                true);
        }

        private static AbstractTransition<T> HandleReplayedCounterexample(
            CheckState<T> state,
            IGenInstance<Test<T>> instance,
            Replay replayDecoded,
            ExplorationStage<Test<T>>.Counterexample counterexample)
        {
            var counterexampleState = new CounterexampleState<T>(
                counterexample.ExampleSpace.Map(x => x.Input),
                instance.ExampleSpaceHistory,
                replayDecoded.GenParameters,
                replayDecoded.ExampleSpacePath,
                counterexample.Exception);

            return new InstanceCompleteTransition<T>(
                state,
                instance,
                counterexampleState,
                false,
                true);
        }

        private static AbstractTransition<T> HandleError(CheckState<T> state)
        {
            return new ErrorTransition<T>(
                state,
                "Error replaying last example, given replay string was no longer valid. Remove the replay " +
                "parameter and run the property again. This is probably due to the generator setup changing.");
        }
    }
}
