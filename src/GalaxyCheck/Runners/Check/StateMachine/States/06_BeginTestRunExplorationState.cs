using GalaxyCheck.ExampleSpaces;
using GalaxyCheck.Gens.Iterations.Generic;

namespace GalaxyCheck.Runners.Check.StateMachine.States
{
    internal record BeginTestRunExplorationState<Test, TestRunExploration>(
        IGenInstance<Test> Instance,
        TestRunExploration InitialTestRunExploration
    ) : TransitionableCheckState<Test>
    {
        public CheckStateTransition<Test> Transition(CheckStateData<Test> checkStateData)
        {
            var initialTestRunExplorationStateData =
                new TestRunExplorationStateData<Test>(Instance, Counterexample: null, IsFirstTestRunForInstance: true);

            return new CheckStateTransition<Test>(
                new HoldingTestRunExplorationState<Test, TestRunExploration>(InitialTestRunExploration,
                    initialTestRunExplorationStateData),
                checkStateData);
        }
    }

    internal record TestRunExplorationStateData<Test>(
        IGenInstance<Test> Instance,
        ExplorationStage<Test>.Counterexample? Counterexample,
        bool IsFirstTestRunForInstance);
}
