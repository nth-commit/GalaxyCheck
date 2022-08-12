using GalaxyCheck.Gens.Internal;
using GalaxyCheck.Gens.Parameters;
using System;

namespace GalaxyCheck
{
    public static partial class Gen
    {
        /// <summary>
        /// Sets the RNG waypoint, that can be used be a later evaluated generator. Useful if you want to control
        /// randomness somewhat, you can use the waypoint as a reference in your generator, independent of what
        /// randomness was consumed between the waypoint being set, and your generator being evaluated,
        /// <see cref="ReferenceRngWaypoint{T}(IGenAdvanced{T}, Func{IRng, IRng})"/>.
        /// </summary>
        /// <param name="gen">The generator to set the waypoint on.</param>
        /// <returns>A new generator, that passes the waypoint through to the given generator.</returns>
        public static IGen<T> SetRngWaypoint<T>(this IGenAdvanced<T> gen) =>
            new FunctionalGen<T>(parameters => gen.Run(parameters with { RngWaypoint = parameters.Rng }));

        /// <summary>
        /// Allows a generator to reference a previously created RNG waypoint, so generation can be controlled.
        /// If a waypoint was previously set, it will be passed to the <paramref name="alignRngToWaypoint"/> function,
        /// from which you can create a new <see cref="IRng"/>. Then, the generator will be ran with that RNG. If a
        /// waypoint was not previously set, it will run the generator as normal.
        /// </summary>
        /// <param name="gen">The underlying generator to run, with respect to the waypoint RNG.</param>
        /// <param name="alignRngToWaypoint">A function used to some new randomness, for which you can control.</param>
        /// <returns>A new generator, which is created from the given generator, and an understanding of the RNG
        /// waypoint options.</returns>
        public static IGen<T> ReferenceRngWaypoint<T>(this IGenAdvanced<T> gen, Func<IRng, IRng> alignRngToWaypoint) =>
            new FunctionalGen<T>(parameters =>
            {
                var rng = parameters.RngWaypoint == null ? parameters.Rng : alignRngToWaypoint(parameters.RngWaypoint);
                return gen.Run(parameters with { Rng = rng });
            });
    }
}
