using GalaxyCheck.Gens.Iterations.Generic;
using GalaxyCheck.ExampleSpaces;
using GalaxyCheck.Runners.Replaying;
using System;
using System.Linq;
using System.Collections.Generic;

namespace GalaxyCheck.Runners.CheckAutomata
{
    internal record ReplayState<T>(
        CheckStateContext<T> Context,
        string ReplayEncoded) : AbstractCheckState<T>(Context)
    {
        internal override AbstractCheckState<T> NextState()
        {
            Replay replayDecoded;
            try
            {
                replayDecoded = ReplayEncoding.Decode(ReplayEncoded);
            }
            catch (Exception ex)
            {
                return new GenerationStates.Error<T>(Context, $"Error decoding replay string: {ex.Message}");
            }

            var iteration = Context.Property.Advanced.Run(replayDecoded.GenParameters).First();

            return iteration.Match(
                onInstance: instance => HandleInstance(Context, instance, replayDecoded),
                onError: error => HandleError(Context),
                onDiscard: _ => HandleError(Context));
        }

        private static AbstractCheckState<T> HandleInstance(CheckStateContext<T> ctx, IGenInstance<Test<T>> instance, Replay replayDecoded)
        {
            var exampleSpace = instance.ExampleSpace.Navigate(replayDecoded.ExampleSpacePath);
            if (exampleSpace == null)
            {
                return HandleError(ctx);
            }

            var exploration = exampleSpace.Explore(AnalyzeExplorationForCheck.Impl<T>()).First();

            return exploration.Match(
                onNonCounterexampleExploration: _ => HandleReplayedNonCounterexample(ctx, instance),
                onCounterexampleExploration: counterexample => HandleReplayedCounterexample(ctx, instance, replayDecoded, counterexample),
                onDiscardExploration: (_) => HandleError(ctx));
        }

        private static AbstractCheckState<T> HandleReplayedNonCounterexample(
            CheckStateContext<T> ctx,
            IGenInstance<Test<T>> instance)
        {
            return new GenerationStates.End<T>(
                ctx,
                instance,
                null,
                false,
                true);
        }

        private static AbstractCheckState<T> HandleReplayedCounterexample(
            CheckStateContext<T> state,
            IGenInstance<Test<T>> instance,
            Replay replayDecoded,
            ExplorationStage<Test<T>>.Counterexample counterexample)
        {
            var counterexampleContext = new CounterexampleContext<T>(
                counterexample.ExampleSpace,
                GetNavigatedExampleSpaceHistory(instance, replayDecoded.ExampleSpacePath),
                replayDecoded.GenParameters,
                replayDecoded.ExampleSpacePath,
                counterexample.Exception);

            return new GenerationStates.End<T>(
                state,
                instance,
                counterexampleContext,
                false,
                true);
        }

        private static AbstractCheckState<T> HandleError(CheckStateContext<T> state)
        {
            return new GenerationStates.Error<T>(
                state,
                "Error replaying last example, given replay string was no longer valid. Remove the replay " +
                "parameter and run the property again. This is probably due to the generator setup changing.");
        }

        private static IEnumerable<Lazy<IExampleSpace<object>?>> GetNavigatedExampleSpaceHistory(
            IGenInstance<Test<T>> instance,
            IEnumerable<int> path) =>
                instance.ExampleSpaceHistory.Select(exs => new Lazy<IExampleSpace<object>?>(() => exs.Cast<object>().Navigate(path)));
    }
}
