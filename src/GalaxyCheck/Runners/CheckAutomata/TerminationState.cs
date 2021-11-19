using GalaxyCheck.Runners.Check;
using System;

namespace GalaxyCheck.Runners.CheckAutomata
{
    internal record TerminationState<T>(
        CheckStateContext<T> Context,
        TerminationReason Reason) : AbstractCheckState<T>(Context)
    {
        internal override AbstractCheckState<T> NextState()
        {
            throw new Exception($"Fatal: Cannot transition from {nameof(TerminationState<T>)}");
        }
    }
}
