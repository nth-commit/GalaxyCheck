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
                    WasDiscard: false);
            }

            return head.Match<AbstractTransition<T>>(
                onCounterexampleExploration: (counterexampleExploration) =>
                    new CounterexampleExplorationTransition<T>(
                        State,
                        Instance,
                        tail,
                        counterexampleExploration),
                onNonCounterexampleExploration: (nonCounterexampleExploration) =>
                    new NonCounterexampleExplorationTransition<T>(
                        State,
                        Instance,
                        tail,
                        nonCounterexampleExploration,
                        CounterexampleState),
                onDiscardExploration: () => IsFirstExplorationStage
                    ? new InstanceCompleteTransition<T>(State, Instance, CounterexampleState: null, WasDiscard: true)
                    : new DiscardExplorationTransition<T>(State, Instance, tail, CounterexampleState));
        }
    }
}
