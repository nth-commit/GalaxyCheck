using GalaxyCheck.Gens.Iterations.Generic;
using GalaxyCheck.Gens.Parameters;
using GalaxyCheck.Runners.Check.Automata;

namespace GalaxyCheck.Runners.Check.Sizing
{
    internal delegate Size ResizeStrategy<T>(ResizeStrategyInformation<T> info);

    internal record ResizeStrategyInformation<T>(
        CheckStateContext<T> CheckStateContext,
        CounterexampleContext<T>? CounterexampleContext,
        IGenInstance<Property.Test<T>> Iteration);
}
