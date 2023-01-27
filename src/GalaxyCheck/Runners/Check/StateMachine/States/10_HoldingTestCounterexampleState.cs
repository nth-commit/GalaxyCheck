using GalaxyCheck.ExampleSpaces;

namespace GalaxyCheck.Runners.Check.StateMachine.States
{
    internal record HoldingTestCounterexampleState<Test, TestRunExploration>(
        TestRunExploration RemainingTestRunExploration,
        TestRunExplorationStateData<Test> TestRunExplorationStateData,
        ExplorationStage<Test>.Counterexample Counterexample
    ) : TransitionableCheckState<Test>
    {
        public CheckStateTransition<Test> Transition(CheckStateData<Test> checkStateData) => new(
            new HoldingTestRunExplorationState<Test, TestRunExploration>(
                RemainingTestRunExploration,
                TestRunExplorationStateData with
                {
                    IsFirstTestRunForInstance = false,
                    Counterexample = Counterexample
                }),
            checkStateData);
    }
}
