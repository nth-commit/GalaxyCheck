using GalaxyCheck.Gens.Iterations.Generic;
using GalaxyCheck.ExampleSpaces;
using System.Collections.Generic;

namespace GalaxyCheck.Runners.CheckAutomata
{
    internal record DiscardExplorationTransition<T>(
        CheckState<T> State,
        IGenInstance<Test<T>> Instance,
        IEnumerable<ExplorationStage<Test<T>>> NextExplorations,
        CounterexampleState<T>? PreviousCounterexample) : AbstractTransition<T>(State)
    {
        internal override AbstractTransition<T> NextTransition() => new InstanceNextExplorationStageTransition<T>(
            State,
            Instance,
            NextExplorations,
            PreviousCounterexample,
            false);
    }
}
