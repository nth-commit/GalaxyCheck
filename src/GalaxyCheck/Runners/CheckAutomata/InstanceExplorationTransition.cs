using GalaxyCheck.Internal.ExampleSpaces;
using GalaxyCheck.Internal.GenIterations;
using System;

namespace GalaxyCheck.Runners.CheckAutomata
{
    public record InstanceExplorationTransition<T>(
        CheckState<T> State,
        IGenInstance<Test<T>> Instance) : AbstractTransition<T>(State)
    {
        internal override AbstractTransition<T> NextTransition()
        {
            AnalyzeExploration<Test<T>> analyze = (example) =>
            {
                try
                {
                    var success = example.Value.Func(example.Value.Input);
                    return success ? ExplorationOutcome.Success() : ExplorationOutcome.Fail(null);
                }
                catch (Property.PropertyPreconditionException)
                {
                    return ExplorationOutcome.Discard();
                }
                catch (Exception ex)
                {
                    return ExplorationOutcome.Fail(ex);
                }
            };

            var explorations = Instance.ExampleSpace.Explore(analyze);
            return new InstanceNextExplorationStageTransition<T>(State, Instance, explorations, null, true);
        }
    }
}
