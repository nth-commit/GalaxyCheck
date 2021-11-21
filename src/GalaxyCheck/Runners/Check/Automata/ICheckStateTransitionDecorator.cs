namespace GalaxyCheck.Runners.Check.Automata
{
    internal interface ICheckStateTransitionDecorator<T>
    {
        CheckStateTransition<T> Decorate(
            CheckState<T> previousState,
            CheckStateTransition<T> nextTransition);
    }
}
