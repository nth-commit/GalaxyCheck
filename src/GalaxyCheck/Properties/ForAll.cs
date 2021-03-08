using GalaxyCheck.Properties;
using System;

namespace GalaxyCheck
{
    public partial class Property
    {
        public static Property<T0> ForAll<T0>(IGen<T0> gen0, Func<T0, bool> func, int? arity = null) =>
            new Property<T0>(gen0, func.Invoke, arity ?? 1);

        public static Property<(T0, T1)> ForAll<T0, T1>(
            IGen<T0> gen0,
            IGen<T1> gen1,
            Func<T0, T1, bool> func) => new Property<(T0, T1)>(
                GalaxyCheck.Gen.Zip(gen0, gen1),
                zipped => func(zipped.Item1, zipped.Item2),
                2);

        public static Property<(T0, T1, T2)> ForAll<T0, T1, T2>(
            IGen<T0> gen0,
            IGen<T1> gen1,
            IGen<T2> gen2,
            Func<T0, T1, T2, bool> func) => new Property<(T0, T1, T2)>(
                GalaxyCheck.Gen.Zip(gen0, gen1, gen2),
                zipped => func(zipped.Item1, zipped.Item2, zipped.Item3),
                3);

        public static Property<(T0, T1, T2, T3)> ForAll<T0, T1, T2, T3>(
            IGen<T0> gen0,
            IGen<T1> gen1,
            IGen<T2> gen2,
            IGen<T3> gen3,
            Func<T0, T1, T2, T3, bool> func) => new Property<(T0, T1, T2, T3)>(
                GalaxyCheck.Gen.Zip(gen0, gen1, gen2, gen3),
                zipped => func(zipped.Item1, zipped.Item2, zipped.Item3, zipped.Item4),
                4);

        public static Property<T0> ForAll<T0>(IGen<T0> gen0, Action<T0> action, int? arity = null) =>
            ForAll(gen0, action.AsTrueFunc(), arity);

        public static Property<(T0, T1)> ForAll<T0, T1>(
            IGen<T0> gen0,
            IGen<T1> gen1,
            Action<T0, T1> action) => ForAll(gen0, gen1, action.AsTrueFunc());

        public static Property<(T0, T1, T2)> ForAll<T0, T1, T2>(
            IGen<T0> gen0,
            IGen<T1> gen1,
            IGen<T2> gen2,
            Action<T0, T1, T2> action) => ForAll(gen0, gen1, gen2, action.AsTrueFunc());

        public static Property<(T0, T1, T2, T3)> ForAll<T0, T1, T2, T3>(
            IGen<T0> gen0,
            IGen<T1> gen1,
            IGen<T2> gen2,
            IGen<T3> gen3,
            Action<T0, T1, T2, T3> action) => ForAll(gen0, gen1, gen2, gen3, action.AsTrueFunc());
    }
}
