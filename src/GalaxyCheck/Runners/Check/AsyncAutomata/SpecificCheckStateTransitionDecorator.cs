namespace GalaxyCheck.Runners.Check.AsyncAutomata
{
    internal abstract class SpecificCheckStateTransitionDecorator<T, TFromState, TToState> : ICheckStateTransitionDecorator<T>
        where TFromState : CheckState<T>
        where TToState : CheckState<T>
    {
        public CheckStateTransition<T> Decorate(CheckState<T> previousState, CheckStateTransition<T> nextTransition)
        {
            if (previousState is TFromState fromState && nextTransition.State is TToState toState)
            {
                return Decorate(fromState, toState, nextTransition.Context);
            }

            return nextTransition;
        }

        protected abstract CheckStateTransition<T> Decorate(TFromState previousState, TToState nextState, CheckStateContext<T> nextContext);
    }
}
