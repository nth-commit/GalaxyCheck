namespace GalaxyCheck.Runners.Check.Automata
{
    internal record CheckStateTransition<T>(
        CheckState<T> State,
        CheckStateContext<T> Context);
}
