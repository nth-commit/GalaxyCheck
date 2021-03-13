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
            var replayDecoded = ReplayEncoding.Decode(ReplayEncoded);

            var iteration = State.Property.Advanced.Run(replayDecoded.GenParameters).First();

            var instance = iteration.Match(
                onInstance: instance => instance,
                onError: _ => throw new Exception("TODO: Handle replayed errors gracefully"),
                onDiscard: _ => throw new Exception("TODO: Handle replayed discards gracefully"));

            var exampleSpace = instance.ExampleSpace.Navigate(replayDecoded.ExampleSpacePath);
            if (exampleSpace == null)
            {
                throw new Exception("TODO: Handle mis-navigation gracefully");
            }

            var exploration = exampleSpace.Explore(AnalyzeExplorationForCheck.Impl<T>()).First();

            var counterexampleState = exploration.Match<CounterexampleState<T>?>(
                onNonCounterexampleExploration: _ => null,
                onCounterexampleExploration: counterexample => new CounterexampleState<T>(
                    counterexample.ExampleSpace.Map(x => x.Input),
                    instance.ExampleSpaceHistory,
                    replayDecoded.GenParameters,
                    replayDecoded.ExampleSpacePath,
                    counterexample.Exception),
                onDiscardExploration: () => throw new Exception("TODO: Handle replayed discard gracefully"));

            return new InstanceCompleteTransition<T>(
                State,
                instance,
                counterexampleState,
                false,
                true);
        }
    }
}
