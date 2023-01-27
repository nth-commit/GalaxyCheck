using System;

namespace GalaxyCheck.Runners.Check.StateMachine.States
{
    internal record ReplayExceptionState<Test>(Exception Exception) : TransitionableCheckState<Test>
    {
        public CheckStateTransition<Test> Transition(CheckStateData<Test> checkStateData)
        {
            throw new NotImplementedException();
        }
    }
}
