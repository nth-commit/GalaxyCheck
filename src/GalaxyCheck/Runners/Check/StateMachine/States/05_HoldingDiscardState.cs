using System.Collections.Generic;
using GalaxyCheck.Gens.Iterations.Generic;

namespace GalaxyCheck.Runners.Check.StateMachine.States
{
    internal record HoldingDiscardState<Test>(
        IGenDiscard<Test> Discard,
        IEnumerable<IGenIteration<Test>> NextIterations
    ) : TransitionableCheckState<Test>
    {
        public CheckStateTransition<Test> Transition(CheckStateData<Test> checkStateData) => new(
            new HoldingIterationsState<Test>(NextIterations),
            checkStateData.IncrementDiscards());
    }
}
