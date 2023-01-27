using System;
using System.Linq;
using GalaxyCheck.ExampleSpaces;
using GalaxyCheck.Gens.Internal.Iterations;
using GalaxyCheck.Gens.Iterations.Generic;
using GalaxyCheck.Runners.Replaying;

namespace GalaxyCheck.Runners.Check.StateMachine.States
{
    internal record ReplayState<Test>(string ReplayEncoded) : TransitionableCheckState<Test>
    {
        public CheckStateTransition<Test> Transition(CheckStateData<Test> checkStateData)
        {
            Replay replayDecoded;
            try
            {
                replayDecoded = ReplayEncoding.Decode(ReplayEncoded);
            }
            catch (Exception ex)
            {
                return HandleInvalidReplayEncoding(checkStateData, ex);
            }

            var iteration = checkStateData.Property.Advanced.Run(replayDecoded.GenParameters).First();
            return iteration.Match(
                onInstance: instance => HandleInstance(checkStateData, instance, replayDecoded),
                onError: error => HandleError(checkStateData, error),
                onDiscard: _ => HandleInvalidatedReplay(checkStateData));
        }

        private static CheckStateTransition<Test> HandleInstance(
            CheckStateData<Test> checkStateData,
            IGenInstance<Test> instance,
            Replay replayDecoded)
        {
            var replayedExampleSpace = instance.ExampleSpace.Navigate(replayDecoded.ExampleSpacePath);
            if (replayedExampleSpace == null)
            {
                return HandleInvalidatedReplay(checkStateData);
            }

            var replayedIteration = GenIterationFactory.Instance(instance.ReplayParameters, instance.NextParameters, replayedExampleSpace);
            var replayedInstance = replayedIteration.Match(
                instance => instance,
                _ => throw new Exception("Fatal"),
                _ => throw new Exception("Fatal"));
            return new CheckStateTransition<Test>(new HoldingInstanceState<Test>(replayedInstance), checkStateData);
        }

        private static CheckStateTransition<Test> HandleError(CheckStateData<Test> checkStateData, IGenError<Test> error) => new(
            HoldingErrorState<Test>.FromGenError(error),
            checkStateData);

        private static CheckStateTransition<Test> HandleInvalidReplayEncoding(CheckStateData<Test> checkStateData, Exception replayEncoded)
        {
            return new CheckStateTransition<Test>(
                new HoldingErrorState<Test>(
                    $"Error decoding replay string: {replayEncoded}",
                    null),
                checkStateData);
        }

        private static CheckStateTransition<Test> HandleInvalidatedReplay(CheckStateData<Test> checkStateData)
        {
            return new CheckStateTransition<Test>(
                new HoldingErrorState<Test>(
                    "Error replaying last example, given replay string was no longer valid. Remove the replay " +
                    "parameter and run the property again. This is probably due to the generator setup changing.",
                    null),
                checkStateData);
        }
    }
}
