using GalaxyCheck.Gens.Iterations.Generic;
using GalaxyCheck.Internal.Utility;
using System;
using System.Collections.Generic;

namespace GalaxyCheck.Runners.CheckAutomata
{
    public record NextIterationTransition<T>(
        CheckState<T> State,
        IEnumerable<IGenIteration<Test<T>>> Iterations) : AbstractTransition<T>(State)
    {
        internal override AbstractTransition<T> NextTransition()
        {
            var (head, tail) = Iterations;
            return head!.Match<AbstractTransition<T>>(
                onInstance: (instance) =>
                    new InstanceExplorationTransition<T>(State, instance),   
                onDiscard: (discard) =>
                    new DiscardTransition<T>(State, tail, discard),
                onError: (error) =>
                    new ErrorTransition<T>(State, $"Error while running generator {error.GenName}: {error.Message}"));
        }
    }
}
