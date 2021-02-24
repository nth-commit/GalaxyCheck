using System;

namespace GalaxyCheck
{
    public static class ForAllExtensions
    {
        /// <summary>
        /// Specify a property of a generator, using the supplied property function.
        /// </summary>
        /// <typeparam name="T">The type of the generator's value.</typeparam>
        /// <param name="gen">The generator to specify a property of.</param>
        /// <param name="func">The property function.</param>
        /// <returns>The property comprised of the generator and the supplied property function.</returns>
        public static Property<T> ForAll<T>(this IGen<T> gen, Func<T, bool> func) => new Property<T>(func.Invoke, gen);

        /// <summary>
        /// Specify a property of a generator, using the supplied property action. With this overload, the implication
        /// is that the property is unfalsified until the given function throws.
        /// </summary>
        /// <typeparam name="T">The type of the generator's value.</typeparam>
        /// <param name="gen">The generator to specify a property of.</param>
        /// <param name="action">The property action.</param>
        /// <returns></returns>
        public static Property<T> ForAll<T>(this IGen<T> gen, Action<T> action) => gen.ForAll(x =>
        {
            action(x);
            return true;
        });
    }

    public static partial class Gen
    {
        public static Property<T0> ForAll<T0>(
            IGen<T0> gen0,
            Func<T0, bool> func) =>
                gen0.ForAll(func);

        public static Property<(T0, T1)> ForAll<T0, T1>(
            IGen<T0> gen0,
            IGen<T1> gen1,
            Func<T0, T1, bool> func) =>
                Zip(gen0, gen1).ForAll(x => func(x.Item1, x.Item2));

        public static Property<(T0, T1, T2)> ForAll<T0, T1, T2>(
            IGen<T0> gen0,
            IGen<T1> gen1,
            IGen<T2> gen2,
            Func<T0, T1, T2, bool> func) =>
                Zip(gen0, gen1, gen2).ForAll(x => func(x.Item1, x.Item2, x.Item3));

        public static Property<(T0, T1, T2, T3)> ForAll<T0, T1, T2, T3>(
            IGen<T0> gen0,
            IGen<T1> gen1,
            IGen<T2> gen2,
            IGen<T3> gen3,
            Func<T0, T1, T2, T3, bool> func) =>
                Zip(gen0, gen1, gen2, gen3).ForAll(x => func(x.Item1, x.Item2, x.Item3, x.Item4));

        public static Property<T0> ForAll<T0>(
            IGen<T0> gen0,
            Action<T0> action) =>
                gen0.ForAll(action);

        public static Property<(T0, T1)> ForAll<T0, T1>(
            IGen<T0> gen0,
            IGen<T1> gen1,
            Action<T0, T1> action) =>
                Zip(gen0, gen1).ForAll(x => action(x.Item1, x.Item2));

        public static Property<(T0, T1, T2)> ForAll<T0, T1, T2>(
            IGen<T0> gen0,
            IGen<T1> gen1,
            IGen<T2> gen2,
            Action<T0, T1, T2> action) =>
                Zip(gen0, gen1, gen2).ForAll(x => action(x.Item1, x.Item2, x.Item3));

        public static Property<(T0, T1, T2, T3)> ForAll<T0, T1, T2, T3>(
            IGen<T0> gen0,
            IGen<T1> gen1,
            IGen<T2> gen2,
            IGen<T3> gen3,
            Action<T0, T1, T2, T3> action) =>
                Zip(gen0, gen1, gen2, gen3).ForAll(x => action(x.Item1, x.Item2, x.Item3, x.Item4));
    }
}
