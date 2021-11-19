namespace GalaxyCheck.Runners.CheckAutomata
{
    internal abstract record AbstractCheckState<T>(CheckStateContext<T> Context)
    {
        internal abstract AbstractCheckState<T> NextState();
    }
}
