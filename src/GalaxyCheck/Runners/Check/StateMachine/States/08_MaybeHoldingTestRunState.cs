using GalaxyCheck.ExampleSpaces;

namespace GalaxyCheck.Runners.Check.StateMachine.States
{
    internal record MaybeHoldingTestRunState<Test, TestRunExploration>(
        ExplorationStage<Test>? TestRun,
        TestRunExploration RemainingTestRunExploration,
        TestRunExplorationStateData<Test> TestRunExplorationStateData
    ) : TransitionableCheckState<Test>
    {
        public CheckStateTransition<Test> Transition(CheckStateData<Test> checkStateData)
        {
            if (TestRun is null)
            {
                return TransitionToEndTestRunExploration(TestRunExplorationStateData, checkStateData);
            }

            if (TestRunExplorationStateData.IsFirstTestRunForInstance is false &&
                checkStateData.TotalShrinks >= checkStateData.ShrinkLimit)
            {
                return TransitionToEndTestRunExploration(TestRunExplorationStateData, checkStateData);
            }

            var nextCheckStateData = TestRunExplorationStateData.IsFirstTestRunForInstance
                ? checkStateData
                : checkStateData.IncrementShrinks();

            return TestRun.Match(
                nonCounterexample => TransitionToHoldingNonCounterexample(
                    RemainingTestRunExploration,
                    TestRunExplorationStateData,
                    nextCheckStateData,
                    nonCounterexample),
                counterexample => TransitionToHoldingCounterexample(
                    RemainingTestRunExploration,
                    TestRunExplorationStateData,
                    nextCheckStateData,
                    counterexample));
        }

        private CheckStateTransition<Test> TransitionToHoldingNonCounterexample(
            TestRunExploration remainingTestRunExploration,
            TestRunExplorationStateData<Test> testRunExplorationStateData,
            CheckStateData<Test> checkStateData,
            ExplorationStage<Test>.NonCounterexample nonCounterexample) =>
            new(
                new HoldingTestNonCounterexampleState<Test, TestRunExploration>(
                    remainingTestRunExploration,
                    testRunExplorationStateData,
                    nonCounterexample),
                checkStateData);

        private CheckStateTransition<Test> TransitionToHoldingCounterexample(
            TestRunExploration remainingTestRunExploration,
            TestRunExplorationStateData<Test> testRunExplorationStateData,
            CheckStateData<Test> checkStateData,
            ExplorationStage<Test>.Counterexample counterexample) =>
            new(
                new HoldingTestCounterexampleState<Test, TestRunExploration>(
                    remainingTestRunExploration,
                    testRunExplorationStateData,
                    counterexample),
                checkStateData);

        private CheckStateTransition<Test> TransitionToEndTestRunExploration(
            TestRunExplorationStateData<Test> testRunExplorationStateData,
            CheckStateData<Test> checkStateData) =>
            new(
                new EndTestRunExplorationState<Test>(testRunExplorationStateData),
                checkStateData);
    }
}
