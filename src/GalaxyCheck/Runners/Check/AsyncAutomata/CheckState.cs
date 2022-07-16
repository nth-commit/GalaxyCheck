using System.Threading.Tasks;

namespace GalaxyCheck.Runners.Check.AsyncAutomata
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    internal interface CheckState<T>
    {
        Task<CheckStateTransition<T>> Transition(CheckStateContext<T> context);
    }
}
