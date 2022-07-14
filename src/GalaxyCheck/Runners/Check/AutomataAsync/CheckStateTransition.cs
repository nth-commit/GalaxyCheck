namespace GalaxyCheck.Runners.Check.AutomataAsync
{
    internal record CheckStateTransition<T>(
        CheckState<T> State,
        CheckStateContext<T> Context);
}
