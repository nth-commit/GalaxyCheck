using GalaxyCheck.Internal.ExampleSpaces;
using GalaxyCheck.Internal.GenIterations;

namespace GalaxyCheck.Runners.CheckAutomata
{
    public record InstanceExplorationTransition<T>(
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
