using GalaxyCheck.Internal.ExampleSpaces;
using GalaxyCheck.Internal.GenIterations;
using GalaxyCheck.Internal.Utility;
using System;
using System.Collections.Generic;
using static GalaxyCheck.CheckExtensions;

namespace GalaxyCheck.Runners.CheckAutomata
{
    public record InstanceNextExplorationStageTransition<T>(
        CheckState<T> State,
        IGenInstance<Test<T>> Instance,
        IEnumerable<ExplorationStage<Test<T>>> Explorations,
        CounterexampleState<T>? CounterexampleState,
        bool IsFirstExplorationStage) : AbstractTransition<T>(State)
    {
        internal override AbstractTransition<T> NextTransition()
        {
            var (head, tail) = Explorations;

            if (head == null)
            {
                return new InstanceCompleteTransition<T>(
                    State,
                    Instance,
                    CounterexampleState,
                    WasDiscard: false,
                    WasReplay: false);
            }

            if (IsFirstExplorationStage == false && State.Shrinks >= State.ShrinkLimit)
            {
                return new InstanceCompleteTransition<T>(
                    State,
                    Instance,
                    CounterexampleState,
                    WasDiscard: false,
                    WasReplay: false);
            }

            var state = IsFirstExplorationStage ? State : State.IncrementShrinks();

            return head.Match<AbstractTransition<T>>(
                onCounterexampleExploration: (counterexampleExploration) =>
                    new CounterexampleExplorationTransition<T>(
                        state,
                        Instance,
                        tail,
                        counterexampleExploration),
                onNonCounterexampleExploration: (nonCounterexampleExploration) =>
                    new NonCounterexampleExplorationTransition<T>(
                        state,
                        Instance,
                        tail,
                        nonCounterexampleExploration,
                        CounterexampleState),
                onDiscardExploration: () => IsFirstExplorationStage
                    ? new InstanceCompleteTransition<T>(state, Instance, CounterexampleState: null, WasDiscard: true, WasReplay: false)
                    : new DiscardExplorationTransition<T>(state, Instance, tail, CounterexampleState));
        }
    }
}
