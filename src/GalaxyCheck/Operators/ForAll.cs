using System;

namespace GalaxyCheck
{
    public static partial class Extensions
    {
        /// <summary>
        /// Specify a property of a generator, using the supplied property function.
        /// </summary>
        /// <typeparam name="T">The type of the generator's value.</typeparam>
        /// <param name="gen">The generator to specify a property of.</param>
        /// <param name="func">The property function.</param>
        /// <returns>The property comprised of the generator and the supplied property function.</returns>
        public static Property<T> ForAll<T>(this IGen<T> gen, Func<T, bool> func, int? arity = null) => Property.ForAll(gen, func, arity);

        /// <summary>
        /// Specify a property of a generator, using the supplied property action. With this overload, the implication
        /// is that the property is unfalsified until the given function throws.
        /// </summary>
        /// <typeparam name="T">The type of the generator's value.</typeparam>
        /// <param name="gen">The generator to specify a property of.</param>
        /// <param name="action">The property action.</param>
        /// <returns></returns>
        public static Property<T> ForAll<T>(this IGen<T> gen, Action<T> action, int? arity = null) => Property.ForAll(gen, action, arity);
    }
}
