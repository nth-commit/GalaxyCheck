using GalaxyCheck;
using GalaxyCheck.Gens.Iterations.Generic;
using GalaxyCheck.ExampleSpaces;
using GalaxyCheck.Runners.Replaying;
using System;
using System.Linq;
using System.Collections.Generic;

namespace GalaxyCheck.Runners.Check.Automata
{
    internal record ReplayState<T>(string ReplayEncoded) : CheckState<T>
    {
        public CheckStateTransition<T> Transition(CheckStateContext<T> context)
        {
            Replay replayDecoded;
            try
            {
                replayDecoded = ReplayEncoding.Decode(ReplayEncoded);
            }
            catch (Exception ex)
            {
                return new CheckStateTransition<T>(
                    new GenerationStates.Generation_Error<T>($"Error decoding replay string: {ex.Message}"),
                    context);
            }

            var iteration = context.Property.Advanced.Run(replayDecoded.GenParameters).First();

            return iteration.Match(
                onInstance: instance => HandleInstance(context, instance, replayDecoded),
                onError: error => HandleError(context),
                onDiscard: _ => HandleError(context));
        }

        private static CheckStateTransition<T> HandleInstance(CheckStateContext<T> ctx, IGenInstance<Test<T>> instance, Replay replayDecoded)
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

        private static CheckStateTransition<T> HandleReplayedNonCounterexample(
            CheckStateContext<T> context,
            IGenInstance<Test<T>> instance)
        {
            return new CheckStateTransition<T>(
                new GenerationStates.Generation_End<T>(instance, null, false, false, true),
                context);
        }

        private static CheckStateTransition<T> HandleReplayedCounterexample(
            CheckStateContext<T> context,
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

            return new CheckStateTransition<T>(
                new GenerationStates.Generation_End<T>(instance, counterexampleContext, false, false, true),
                context);
        }

        private static CheckStateTransition<T> HandleError(CheckStateContext<T> context)
        {
            return new CheckStateTransition<T>(
                new GenerationStates.Generation_Error<T>(
                    "Error replaying last example, given replay string was no longer valid. Remove the replay " +
                    "parameter and run the property again. This is probably due to the generator setup changing."),
                context);
        }

        private static IEnumerable<Lazy<IExampleSpace<object>?>> GetNavigatedExampleSpaceHistory(
            IGenInstance<Test<T>> instance,
            IEnumerable<int> path) =>
                instance.ExampleSpaceHistory.Select(exs => new Lazy<IExampleSpace<object>?>(() => exs.Cast<object>().Navigate(path)));
    }
}
