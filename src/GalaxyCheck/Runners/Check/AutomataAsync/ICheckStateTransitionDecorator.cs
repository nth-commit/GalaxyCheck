namespace GalaxyCheck.Runners.Check.AutomataAsync
{
    internal interface ICheckStateTransitionDecorator<T>
    {
        CheckStateTransition<T> Decorate(
            CheckState<T> previousState,
            CheckStateTransition<T> nextTransition);
    }
}
