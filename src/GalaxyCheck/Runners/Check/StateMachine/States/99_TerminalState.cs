
namespace GalaxyCheck.Runners.Check.StateMachine.States
{
    internal record TerminalState<Test>(TerminationReason Reason) : InterruptedCheckState<Test>;
}
