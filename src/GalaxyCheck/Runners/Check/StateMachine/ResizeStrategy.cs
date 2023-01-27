using GalaxyCheck.ExampleSpaces;
using GalaxyCheck.Gens.Iterations.Generic;
using GalaxyCheck.Gens.Parameters;
using GalaxyCheck.Runners.Check.StateMachine.States;

namespace GalaxyCheck.Runners.Check.StateMachine
{
    internal delegate Size ResizeStrategy<Test>(ResizeStrategyInformation<Test> info);

    internal record ResizeStrategyInformation<Test>(
        CheckStateData<Test> CheckStateData,
        ExplorationStage<Test>.Counterexample? CurrentCounterexample,
        IGenInstance<Test> Instance);
}
