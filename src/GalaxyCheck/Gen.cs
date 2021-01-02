using GalaxyCheck.Abstractions;
using GalaxyCheck.ExampleSpaces;
using GalaxyCheck.Gens;

namespace GalaxyCheck
{
    public static class Gen
    {
        /// <summary>
        /// Creates a generator that always produces the given value.
        /// </summary>
        /// <typeparam name="T">The type of the generator's value.</typeparam>
        /// <param name="value">The constant value the generator should produce.</param>
        /// <returns>The new generator.</returns>
        public static IGenBuilder<T> Constant<T>(T value) => PrimitiveGenBuilder.Create(
            (useNextInt) => value,
            ShrinkFunc.None<T>(),
            MeasureFunc.Unmeasured<T>());
    }
}
