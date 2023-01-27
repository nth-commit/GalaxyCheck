namespace GalaxyCheck.Runners.Check.StateMachine.States
{
    internal record HoldingTestRunExplorationState<Test, TestRunExploration>(
        TestRunExploration RemainingTestRunExploration,
        TestRunExplorationStateData<Test> TestRunExplorationStateData
    ) : InterruptedCheckState<Test>;
}
