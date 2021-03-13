using GalaxyCheck.Internal.ExampleSpaces;
using GalaxyCheck.Internal.GenIterations;
using System.Collections.Generic;
using static GalaxyCheck.CheckExtensions;

namespace GalaxyCheck.Runners.CheckAutomata
{
    public record DiscardExplorationTransition<T>(
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
