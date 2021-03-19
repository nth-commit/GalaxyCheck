using GalaxyCheck.Injection;
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
            /// Generated values should scale exponentially with the size parameter.
            /// </summary>
            WithSize = 2
        }

        /// <summary>
        /// Creates a generator that produces parameters to the given method. Can be used to dynamically invoke a
        /// method or a delegate.
        /// </summary>
        /// <param name="methodInfo">The method to generate parameters for.</param>
        /// <returns>The new generator.</returns>
        public static IGen<object[]> Parameters(MethodInfo methodInfo) => new ParametersGen(methodInfo);

        /// <summary>
        /// A container which contains generators for advanced usage.
        /// </summary>
        public static partial class Advanced
        {
        }
    }
}
