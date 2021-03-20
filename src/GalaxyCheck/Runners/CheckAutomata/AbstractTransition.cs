namespace GalaxyCheck.Runners.CheckAutomata
{
    internal abstract record AbstractTransition<T>(CheckState<T> State)
    {
        internal abstract AbstractTransition<T> NextTransition();
    }
}
