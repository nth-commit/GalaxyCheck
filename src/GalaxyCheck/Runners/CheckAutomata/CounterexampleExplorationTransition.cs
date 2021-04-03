using GalaxyCheck.Gens.Iterations.Generic;
using GalaxyCheck.ExampleSpaces;
using System.Collections.Generic;

namespace GalaxyCheck.Runners.CheckAutomata
{
    internal record CounterexampleExplorationTransition<T>(
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
            Instance.ReplayParameters,
            CounterexampleExploration.Path,
            CounterexampleExploration.Exception,
            CounterexampleExploration.ExampleSpace.Current.Value.EnableLinqInference);
    }
}
