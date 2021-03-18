﻿namespace GalaxyCheck.Runners.CheckAutomata
{
    public record ErrorTransition<T>(
        CheckState<T> State,
        string Error) : AbstractTransition<T>(State)
    {
        internal override AbstractTransition<T> NextTransition() => new Termination<T>(State, TerminationReason.FoundError);
    }
}
