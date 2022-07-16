namespace GalaxyCheck.Runners.Check.AsyncAutomata
{
    internal record CheckStateTransition<T>(
        CheckState<T> State,
        CheckStateContext<T> Context);
}
