using GalaxyCheck.Gens;
using GalaxyCheck.Injection;
using GalaxyCheck.Internal.ExampleSpaces;
using GalaxyCheck.Internal.Gens;
using System;
using System.Linq;
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
        /// Creates a generator that always produces the given value.
        /// </summary>
        /// <typeparam name="T">The type of the generator's value.</typeparam>
        /// <param name="value">The constant value the generator should produce.</param>
        /// <returns>The new generator.</returns>
        public static IGen<T> Constant<T>(T value) => Advanced.Create((useNextInt, size) => value);

        /// <summary>
        /// Creates a generator that produces parameters to the given method. Can be used to dynamically invoke a
        /// method or a delegate.
        /// </summary>
        /// <param name="methodInfo">The method to generate parameters for.</param>
        /// <returns>The new generator.</returns>
        public static IGen<object[]> Parameters(MethodInfo methodInfo) => new ParametersGen(methodInfo);

        public static partial class Advanced
        {
            public static IGen<T> Create<T>(StatefulGenFunc<T> generate) => new PrimitiveGen<T>(generate);
        }
    }
}
