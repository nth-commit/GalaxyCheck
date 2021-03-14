using GalaxyCheck.Internal.ExampleSpaces;
using GalaxyCheck.Internal.GenIterations;
using System.Collections.Generic;
using static GalaxyCheck.CheckExtensions;

namespace GalaxyCheck.Runners.CheckAutomata
{
    public record CounterexampleExplorationTransition<T>(
        CheckState<T> State,
        IGenInstance<Test<T>> Instance,
        IEnumerable<ExplorationStage<Test<T>>> NextExplorations,
        ExplorationStage<Test<T>>.Counterexample CounterexampleExploration) : AbstractTransition<T>(State)
    {
        internal override AbstractTransition<T> NextTransition() => new InstanceNextExplorationStageTransition<T>(
            State,
            Instance,
            NextExplorations,
            CounterexampleState,
            IsFirstExplorationStage: false);

        public CounterexampleState<T> CounterexampleState => new CounterexampleState<T>(
            CounterexampleExploration.ExampleSpace.Map(ex => ex.Input),
            Instance.ExampleSpaceHistory,
            Instance.RepeatParameters,
            CounterexampleExploration.Path,
            CounterexampleExploration.Exception);
    }
}
