using GalaxyCheck.Runners.Check.AutomataAsync;

namespace GalaxyCheck.Runners.Check.Sizing
{
    internal class ResizeCheckStateTransitionDecoratorAsync<T> :
        SpecificCheckStateTransitionDecorator<T, GenerationStates.Generation_End<T>, GenerationStates.Generation_Begin<T>>,
        ICheckStateTransitionDecorator<T>
    {
        private const int MaxConsecutiveDiscards = 10;

        private readonly ResizeStrategyAsync<T> _resizeStrategy;

        public ResizeCheckStateTransitionDecoratorAsync(ResizeStrategyAsync<T> resizeStrategy)
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
            ResizeStrategyAsync<T> resizeStrategy,
            GenerationStates.Generation_End<T> generationEndState,
            CheckStateContext<T> nextContext)
        {
            if (generationEndState.WasLateDiscard)
            {
                if (nextContext.ConsecutiveLateDiscards >= MaxConsecutiveDiscards)
                {
                    return nextContext
                        .WithNextGenParameters(nextContext.NextParameters with
                        {
                            Size = nextContext.NextParameters.Size.BigIncrement()
                        })
                        .ResetConsecutiveDiscards();
                }
                else
                {
                    return nextContext;
                }
            }
            else
            {
                var resizeStrategyInfo = new ResizeStrategyInformationAsync<T>(
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
