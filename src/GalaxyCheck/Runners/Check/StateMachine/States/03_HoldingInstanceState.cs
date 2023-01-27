using GalaxyCheck.Gens.Iterations.Generic;

namespace GalaxyCheck.Runners.Check.StateMachine.States
{
    internal record HoldingInstanceState<Test>(IGenInstance<Test> Instance) : InterruptedCheckState<Test>;
}
