namespace GalaxyCheck.Runners.Check.StateMachine.States
{
    internal record BeginGenerationState<Test> : TransitionableCheckState<Test>
    {
        public CheckStateTransition<Test> Transition(CheckStateData<Test> checkStateData)
        {
            if (checkStateData.CompletedIterations >= checkStateData.RequestedIterations)
            {
                return new CheckStateTransition<Test>(
                    new TerminalState<Test>(TerminationReason.ReachedMaximumIterations),
                    checkStateData);
            }

            var iterations = checkStateData.Property.Advanced.Run(checkStateData.NextParameters);

            return new CheckStateTransition<Test>(
                new HoldingIterationsState<Test>(iterations),
                checkStateData);
        }
    }
}
