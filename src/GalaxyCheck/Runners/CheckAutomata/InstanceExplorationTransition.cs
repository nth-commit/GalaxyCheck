using GalaxyCheck.Gens.Iterations.Generic;
using GalaxyCheck.ExampleSpaces;

namespace GalaxyCheck.Runners.CheckAutomata
{
    internal record InstanceExplorationTransition<T>(
        CheckState<T> State,
        IGenInstance<Test<T>> Instance) : AbstractTransition<T>(State)
    {
        internal override AbstractTransition<T> NextTransition()
        {
            var explorations = Instance.ExampleSpace.Explore(AnalyzeExplorationForCheck.Impl<T>());

            return new InstanceNextExplorationStageTransition<T>(State, Instance, explorations, null, true);
        }
    }
}
