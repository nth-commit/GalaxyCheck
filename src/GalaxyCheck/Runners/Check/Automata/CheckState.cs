namespace GalaxyCheck.Runners.Check.Automata
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    internal interface CheckState<T>
    {
        CheckStateTransition<T> Transition(CheckStateContext<T> context);
    }
}
