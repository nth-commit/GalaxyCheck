using System;
using System.Threading.Tasks;

namespace GalaxyCheck
{
    public static partial class Extensions
    {
        /// <summary>
        /// Specify a property of a generator, using the supplied asynchronous property function.
        /// </summary>
        /// <typeparam name="T">The type of the generator's value.</typeparam>
        /// <param name="gen">The generator to specify a property of.</param>
        /// <param name="func">The property function (async).</param>
        /// <returns>The property comprised of the generator and the supplied property function.</returns>
        public static AsyncProperty<T> ForAllAsync<T>(this IGen<T> gen, Func<T, Task<bool>> func) => Property.ForAllAsync(gen, func);

        /// <summary>
        /// Specify a property of a generator, using the supplied asynchronous  property action. With this overload, the
        /// implication is that the property is unfalsified until the given function throws.
        /// </summary>
        /// <typeparam name="T">The type of the generator's value.</typeparam>
        /// <param name="gen">The generator to specify a property of.</param>
        /// <param name="action">The property action (async).</param>
        /// <returns></returns>
        public static AsyncProperty<T> ForAllAsync<T>(this IGen<T> gen, Func<T, Task> action) => Property.ForAllAsync(gen, action);
    }
}
