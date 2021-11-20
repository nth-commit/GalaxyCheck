namespace GalaxyCheck.Runners.CheckAutomata
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    internal interface CheckState<T>
    {
        CheckStateTransition<T> Transition(CheckStateContext<T> context);
    }

    internal record CheckStateTransition<T>(
        CheckState<T> NextState,
        CheckStateContext<T> NextContext);
}
