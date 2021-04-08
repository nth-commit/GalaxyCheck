using GalaxyCheck.Gens.Iterations.Generic;
using GalaxyCheck.ExampleSpaces;
using System.Collections.Generic;
using System.Linq;
using System;

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
            ExampleSpaceHistory,
            Instance.ReplayParameters,
            CounterexampleExploration.Path,
            CounterexampleExploration.Exception);

        public IExampleSpace<Test<T>> TestExampleSpace => CounterexampleExploration.ExampleSpace;

        public IExampleSpace<T> InputExampleSpace => CounterexampleExploration.ExampleSpace.Map(ex => ex.Input);

        public IEnumerable<int> Path => CounterexampleExploration.Path;

        public IEnumerable<Lazy<IExampleSpace<object>?>> ExampleSpaceHistory => Instance.ExampleSpaceHistory.Select(
            exs => new Lazy<IExampleSpace<object>?>(() => exs.Cast<object>().Navigate(Path)));
    }
}
