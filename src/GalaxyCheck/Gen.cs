using GalaxyCheck.Injection;
using GalaxyCheck.Internal.ExampleSpaces;
using GalaxyCheck.Internal.Gens;
using System.Reflection;

namespace GalaxyCheck
{
    public static partial class Gen
    {
        /// <summary>
        /// A flag which indicates how values produced by generators should scale with respect to the size parameter.
        /// </summary>
        public enum Bias
        {
            /// <summary>
            /// Generated values should not be biased by the size parameter.
            /// </summary>
            None = 0,

            /// <summary>
            /// Generated values should scale linearly with the size parameter.
            /// </summary>
            Linear = 1,

            /// <summary>
            /// Generated values should scale exponentially with the size parameter.
            /// </summary>
            Exponential = 2
        }

        /// <summary>
        /// Creates a generator that always produces the given value.
        /// </summary>
        /// <typeparam name="T">The type of the generator's value.</typeparam>
        /// <param name="value">The constant value the generator should produce.</param>
        /// <returns>The new generator.</returns>
        public static IGen<T> Constant<T>(T value) => Advanced.Create(
            (useNextInt, size) => value,
            identify: IdentifyFuncs.Constant<T>());

        /// <summary>
        /// Creates a generator that produces 32-bit integers. By default, it will generate integers in the full range
        /// (-2,147,483,648 to 2,147,483,647), but the generator returned contains configuration methods to constrain
        /// the produced integers further.
        /// </summary>
        /// <returns>The new generator.</returns>
        public static IInt32Gen Int32() => new Int32Gen();

        /// <summary>
        /// Creates a generator that produces parameters to the given method. Can be used to dynamically invoke a
        /// method or a delegate.
        /// </summary>
        /// <param name="methodInfo">The method to generate parameters for.</param>
        /// <returns>The new generator.</returns>
        public static IGen<object[]> Parameters(MethodInfo methodInfo) => new ParametersGen(methodInfo);

        public static class Advanced
        {
            public static IGen<T> Create<T>(
                StatefulGenFunc<T> generate,
                ShrinkFunc<T>? shrink = null,
                MeasureFunc<T>? measure = null,
                IdentifyFunc<T>? identify = null) => new PrimitiveGen<T>(
                    generate,
                    shrink ?? ShrinkFunc.None<T>(),
                    measure ?? MeasureFunc.Unmeasured<T>(),
                    identify ?? IdentifyFuncs.Default<T>());
        }
    }
}
