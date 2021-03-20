using GalaxyCheck.Gens.Iterations.Generic;
using GalaxyCheck.Internal.ExampleSpaces;
using System.Collections.Generic;

namespace GalaxyCheck.Runners.CheckAutomata
{
    internal record NonCounterexampleExplorationTransition<T>(
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
