using GalaxyCheck.Runners.Check.Automata;

namespace GalaxyCheck.Runners.Check.Sizing
{
    internal class ResizeCheckStateTransitionDecorator<T> :
        SpecificCheckStateTransitionDecorator<T, GenerationStates.Generation_End<T>, GenerationStates.Generation_Begin<T>>,
        ICheckStateTransitionDecorator<T>
    {
        private const int MaxConsecutiveDiscards = 10;

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
            var nextContextResized = Resize(_resizeStrategy, generationEndState, nextContext);

            return new CheckStateTransition<T>(generationBeginState, nextContextResized);
        }

        private static CheckStateContext<T> Resize(
            ResizeStrategy<T> resizeStrategy,
            GenerationStates.Generation_End<T> generationEndState,
            CheckStateContext<T> nextContext)
        {
            if (nextContext.ConsecutiveLateDiscards >= MaxConsecutiveDiscards)
            {
                return nextContext
                    .WithNextGenParameters(nextContext.NextParameters with { Size = nextContext.NextParameters.Size.BigIncrement() })
                    .ResetConsecutiveLateDiscards();
            }
            else
            {
                var resizeStrategyInfo = new ResizeStrategyInformation<T>(
                    nextContext,
                    generationEndState.CounterexampleContext,
                    generationEndState.Instance);

                var nextSize = resizeStrategy(resizeStrategyInfo);

                return nextContext
                    .WithNextGenParameters(nextContext.NextParameters with { Size = nextSize });
            }
        }
    }
}
