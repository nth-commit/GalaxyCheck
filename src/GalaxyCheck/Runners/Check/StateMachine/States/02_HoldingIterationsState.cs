using System.Collections.Generic;
using GalaxyCheck.Gens.Iterations.Generic;
using GalaxyCheck.Internal;

namespace GalaxyCheck.Runners.Check.StateMachine.States
{
    internal record HoldingIterationsState<Test>(IEnumerable<IGenIteration<Test>> Iterations)
        : TransitionableCheckState<Test>
    {
        public CheckStateTransition<Test> Transition(CheckStateData<Test> checkStateData)
        {
            var (head, tail) = Iterations;

            var nextState = head!.Match<CheckState<Test>>(
                instance => new HoldingInstanceState<Test>(instance),
                HoldingErrorState<Test>.FromGenError,
                discard => new HoldingDiscardState<Test>(discard, tail));

            return new CheckStateTransition<Test>(nextState, checkStateData);
        }
    }
}
