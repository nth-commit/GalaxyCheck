namespace GalaxyCheck.Runners.Check.StateMachine.States
{
    internal record EndTestRunExplorationState<Test>(
        TestRunExplorationStateData<Test> TestRunExplorationStateData
    ) : TransitionableCheckState<Test>
    {
        public CheckStateTransition<Test> Transition(CheckStateData<Test> checkStateData) => new(
            new EndGenerationState<Test>(
                TestRunExplorationStateData.Instance,
                TestRunExplorationStateData.Counterexample),
            checkStateData);
    }
}
