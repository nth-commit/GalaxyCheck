using GalaxyCheck.Gens.Iterations.Generic;
using GalaxyCheck.ExampleSpaces;
using System.Collections.Generic;
using System;
using System.Linq;

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

        public IExampleSpace<Test<T>> TestExampleSpace => NonCounterexampleExploration.ExampleSpace;

        public IExampleSpace<T> InputExampleSpace => NonCounterexampleExploration.ExampleSpace.Map(ex => ex.Input);

        public IEnumerable<int> Path => NonCounterexampleExploration.Path;

        public IEnumerable<Lazy<IExampleSpace<object>?>> ExampleSpaceHistory => Instance.ExampleSpaceHistory.Select(
            exs => new Lazy<IExampleSpace<object>?>(() => exs.Cast<object>().Navigate(Path)));
    }
}
