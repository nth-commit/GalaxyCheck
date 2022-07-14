using System;
using System.Threading.Tasks;

namespace GalaxyCheck.Runners.Check.AutomataAsync
{
    internal record TerminationState<T>(TerminationReason Reason) : CheckState<T>
    {
        public Task<CheckStateTransition<T>> Transition(CheckStateContext<T> context)
        {
            throw new Exception($"Fatal: Cannot transition from {nameof(TerminationState<T>)}");
        }
    }
}
