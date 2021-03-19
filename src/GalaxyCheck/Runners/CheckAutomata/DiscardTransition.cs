using GalaxyCheck.Gens.Iterations.Generic;
using System.Collections.Generic;

namespace GalaxyCheck.Runners.CheckAutomata
{
    public record DiscardTransition<T>(
        CheckState<T> State,
        IEnumerable<IGenIteration<Test<T>>> NextIterations,
        IGenDiscard<Test<T>> Discard) : AbstractTransition<T>(State)
    {
        internal override AbstractTransition<T> NextTransition()
        {
            var nextState = State.IncrementDiscards();
            return new NextIterationTransition<T>(nextState, NextIterations);
        }
    }
}
