using GalaxyCheck.Gens.Iterations.Generic;
using GalaxyCheck.Gens.Parameters;

namespace GalaxyCheck.Runners.Check.StateMachine.States
{
    internal record HoldingErrorState<Test>(string Description, GenParameters? ReplayParameters) : TransitionableCheckState<Test>
    {
        public static HoldingErrorState<Test> FromGenError(IGenError<Test> Error) =>
            new($"Error during generation: {Error.Message}", Error.ReplayParameters);

        public CheckStateTransition<Test> Transition(CheckStateData<Test> checkStateData) => new(
            new TerminalState<Test>(TerminationReason.FoundError),
            checkStateData);
    }
}
