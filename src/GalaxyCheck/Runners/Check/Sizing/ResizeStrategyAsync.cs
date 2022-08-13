using GalaxyCheck.Gens.Iterations.Generic;
using GalaxyCheck.Gens.Parameters;
using GalaxyCheck.Runners.Check.AsyncAutomata;

namespace GalaxyCheck.Runners.Check.Sizing
{
    internal delegate Size ResizeStrategyAsync<T>(ResizeStrategyInformationAsync<T> info);

    internal record ResizeStrategyInformationAsync<T>(
        CheckStateContext<T> CheckStateContext,
        CounterexampleContext<T>? CounterexampleContext,
        IGenInstance<Property.AsyncTest<T>> Iteration);
}
