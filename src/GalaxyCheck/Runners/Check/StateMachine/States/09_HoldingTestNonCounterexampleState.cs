using GalaxyCheck.ExampleSpaces;

namespace GalaxyCheck.Runners.Check.StateMachine.States
{
    internal record HoldingTestNonCounterexampleState<Test, TestRunExploration>(
        TestRunExploration RemainingTestRunExploration,
        TestRunExplorationStateData<Test> TestRunExplorationStateData,
        ExplorationStage<Test>.NonCounterexample NonCounterexample
    ) : TransitionableCheckState<Test>
    {
        public CheckStateTransition<Test> Transition(CheckStateData<Test> checkStateData) => new(
            new HoldingTestRunExplorationState<Test, TestRunExploration>(
                RemainingTestRunExploration,
                TestRunExplorationStateData with { IsFirstTestRunForInstance = false }),
            checkStateData);
    }
}
