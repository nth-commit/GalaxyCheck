namespace GalaxyCheck.Runners.CheckAutomata
{
    public abstract record AbstractTransition<T>(CheckState<T> State)
    {
        internal abstract AbstractTransition<T> NextTransition();
    }
}
