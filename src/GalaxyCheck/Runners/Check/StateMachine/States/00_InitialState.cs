namespace GalaxyCheck.Runners.Check.StateMachine.States
{
    internal record InitialState<Test> : TransitionableCheckState<Test>
    {
        public CheckStateTransition<Test> Transition(CheckStateData<Test> checkStateData)
        {
            return new CheckStateTransition<Test>(
                new BeginGenerationState<Test>(),
                checkStateData);
        }
    }
}
