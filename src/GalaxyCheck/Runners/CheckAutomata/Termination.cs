using System;

namespace GalaxyCheck.Runners.CheckAutomata
{
    public record Termination<T>(CheckState<T> State) : AbstractTransition<T>(State)
    {
        internal override AbstractTransition<T> NextTransition()
        {
            throw new Exception("Fatal: Cannot transition from termination");
        }
    }
}
