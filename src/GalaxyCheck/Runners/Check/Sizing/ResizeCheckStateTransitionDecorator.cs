using GalaxyCheck.Runners.Check.Automata;

namespace GalaxyCheck.Runners.Check.Sizing
{
    internal class ResizeCheckStateTransitionDecorator<T> :
        SpecificCheckStateTransitionDecorator<T, GenerationStates.Generation_End<T>, GenerationStates.Generation_Begin<T>>,
        ICheckStateTransitionDecorator<T>
    {
        private readonly ResizeStrategy<T> _resizeStrategy;

        public ResizeCheckStateTransitionDecorator(ResizeStrategy<T> resizeStrategy)
        {
            _resizeStrategy = resizeStrategy;
        }

        protected override CheckStateTransition<T> Decorate(
            GenerationStates.Generation_End<T> generationEndState,
            GenerationStates.Generation_Begin<T> generationBeginState,
            CheckStateContext<T> nextContext)
        {
            var resizeStrategyInfo = new ResizeStrategyInformation<T>(
                nextContext,
                generationEndState.CounterexampleContext,
                generationEndState.Instance);

            var nextSize = _resizeStrategy(resizeStrategyInfo);

            return new CheckStateTransition<T>(
                generationBeginState,
                nextContext.WithNextGenParameters(nextContext.NextParameters with { Size = nextSize }));
        }
    }
}
