namespace GalaxyCheck.Runners.Check.AsyncAutomata
{
    internal interface ICheckStateTransitionDecorator<T>
    {
        CheckStateTransition<T> Decorate(
            CheckState<T> previousState,
            CheckStateTransition<T> nextTransition);
    }
}
