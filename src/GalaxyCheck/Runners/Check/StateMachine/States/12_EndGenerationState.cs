using System.Linq;
using GalaxyCheck.ExampleSpaces;
using GalaxyCheck.Gens.Iterations.Generic;

namespace GalaxyCheck.Runners.Check.StateMachine.States
{
    internal record EndGenerationState<Test>(
        IGenInstance<Test> Instance,
        ExplorationStage<Test>.Counterexample? Counterexample
    ) : TransitionableCheckState<Test>
    {
        public CheckStateTransition<Test> Transition(CheckStateData<Test> checkStateData)
        {
            return ApplyResizeStrategy(TransitionWithoutResize(checkStateData));
        }

        private CheckStateTransition<Test> TransitionWithoutResize(CheckStateData<Test> checkStateData)
        {
            checkStateData = checkStateData.WithNextGenParameters(Instance.NextParameters);

            var counterexample = Counterexample ?? checkStateData.Counterexample;
            if (counterexample is null)
            {
                return TransitionToBeginGenerationState(checkStateData.IncrementCompletedIterations());
            }

            return TransitionWithCounterexample(counterexample, checkStateData);
        }

        private CheckStateTransition<Test> ApplyResizeStrategy(CheckStateTransition<Test> transition)
        {
            return transition with
            {
                NextStateData = transition.NextStateData with
                {
                    NextParameters = transition.NextStateData.NextParameters with
                    {
                        Size = transition.NextStateData.ResizeStrategy(
                            new ResizeStrategyInformation<Test>(transition.NextStateData, Counterexample, Instance))
                    }
                }
            };
        }

        private CheckStateTransition<Test> TransitionToBeginGenerationState(CheckStateData<Test> checkStateData) =>
            new(
                new BeginGenerationState<Test>(),
                checkStateData);

        private CheckStateTransition<Test> TransitionWithCounterexample(
            ExplorationStage<Test>.Counterexample newCounterexample,
            CheckStateData<Test> checkStateData)
        {
            checkStateData = checkStateData
                .IncrementCompletedIterations()
                .AddCounterexample(newCounterexample, Instance.ReplayParameters);

            var terminationReason = ShouldTerminate(Instance, checkStateData);
            if (terminationReason is null)
            {
                return TransitionToBeginGenerationState(checkStateData);
            }

            checkStateData = checkStateData with
            {
                NextParameters = checkStateData.NextParameters with
                {
                    Size = checkStateData.ResizeStrategy(
                        new ResizeStrategyInformation<Test>(checkStateData, newCounterexample, Instance))
                }
            };

            return new CheckStateTransition<Test>(new TerminalState<Test>(terminationReason.Value), checkStateData);
        }

        private static TerminationReason? ShouldTerminate(
            IGenInstance<Test> instance,
            CheckStateData<Test> checkStateData)
        {
            if (checkStateData.IsReplay)
            {
                // We just want to repeat the last failure. The check we are replaying presumably reached for the
                // smallest counterexample anyway.
                return TerminationReason.IsReplay;
            }

            if (!checkStateData.DeepCheck)
            {
                return TerminationReason.DeepCheckDisabled;
            }

            if (checkStateData.CompletedIterations >= checkStateData.RequestedIterations)
            {
                // We should stop reaching for smaller counterexamples if we are at max iterations already.
                return TerminationReason.ReachedMaximumIterations;
            }

            if (checkStateData.TotalShrinks >= checkStateData.ShrinkLimit)
            {
                return TerminationReason.ReachedShrinkLimit;
            }

            if (instance.ReplayParameters.Size.Value == 100)
            {
                // We're at the max size, don't bother starting again from 0% as the compute time is most likely not
                // worth it. TODO: Add config to disable this if someone REALLY wants to find a smaller counterexample.
                return TerminationReason.ReachedMaximumSize;
            }

            var lastCounterexample = checkStateData.Counterexamples.Last();
            if (lastCounterexample.ExampleSpace.Current.Distance == 0)
            {
                // The counterexample is literally the smallest possible example that fits the constraints.
                return TerminationReason.FoundTheoreticalSmallestCounterexample;
            }

            const int recentCounterexampleThreshold = 3;
            var recentCounterexamples = checkStateData.Counterexamples.Take(recentCounterexampleThreshold).ToList();
            if (recentCounterexamples.Count == recentCounterexampleThreshold &&
                recentCounterexamples.GroupBy(x => x.ExampleSpace.Current.Id).Count() == 1)
            {
                // The recent counterexamples have settled somewhat, short-circuit the remaining iterations
                return TerminationReason.FoundPragmaticSmallestCounterexample;
            }

            return null;
        }
    }
}
