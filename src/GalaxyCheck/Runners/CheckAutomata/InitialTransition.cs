using GalaxyCheck.Runners.Check;

namespace GalaxyCheck.Runners.CheckAutomata
{
    internal record InitialTransition<T>(CheckState<T> State) : AbstractTransition<T>(State)
    {
        internal override AbstractTransition<T> NextTransition()
        {
            if (State.CompletedIterations >= State.RequestedIterations)
            {
                return new Termination<T>(State, TerminationReason.ReachedMaximumIterations);
            }

            var iterations = State.Property.Advanced.Run(State.NextParameters);
            return new NextIterationTransition<T>(State, iterations);
        }
    }
}
