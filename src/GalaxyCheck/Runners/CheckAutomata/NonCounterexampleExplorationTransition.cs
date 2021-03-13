using GalaxyCheck.Internal.ExampleSpaces;
using GalaxyCheck.Internal.GenIterations;
using System.Collections.Generic;
using static GalaxyCheck.CheckExtensions;

namespace GalaxyCheck.Runners.CheckAutomata
{
    public record NonCounterexampleExplorationTransition<T>(
        CheckState<T> State,
        IGenInstance<Test<T>> Instance,
        IEnumerable<ExplorationStage<Test<T>>> NextExplorations,
        ExplorationStage<Test<T>>.NonCounterexample NonCounterexampleExploration,
        CounterexampleState<T>? PreviousCounterexampleState) : AbstractTransition<T>(State)
    {
        internal override AbstractTransition<T> NextTransition() => new InstanceNextExplorationStageTransition<T>(
            State,
            Instance,
            NextExplorations,
            PreviousCounterexampleState,
            false);
    }
}
