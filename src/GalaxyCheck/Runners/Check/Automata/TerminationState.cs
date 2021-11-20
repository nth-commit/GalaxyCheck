using System;

namespace GalaxyCheck.Runners.Check.Automata
{
    internal record TerminationState<T>(TerminationReason Reason) : CheckState<T>
    {
        public CheckStateTransition<T> Transition(CheckStateContext<T> context)
        {
            throw new Exception($"Fatal: Cannot transition from {nameof(TerminationState<T>)}");
        }
    }
}
