using GalaxyCheck.Properties;
using System;

namespace GalaxyCheck
{
    public partial class Property
    {
        public static Property<object[]> Nullary(Func<bool> func) => new Property<object[]>(
            Gen.Constant(new object[] { }).Select(x => TestFactory.Create<object[]>(
                x,
                func,
                x)));

        public static Property<T0> ForAll<T0>(IGen<T0> gen0, Func<T0, bool> func) =>
            new Property<T0>(
                gen0.Select(x => TestFactory.Create(
                    x,
                    () => func(x),
                    new object?[] { x })));

        public static Property<(T0, T1)> ForAll<T0, T1>(
            IGen<T0> gen0,
            IGen<T1> gen1,
            Func<T0, T1, bool> func) => new Property<(T0, T1)>(
                Gen.Zip(gen0, gen1).Select(tuple => TestFactory.Create(
                    tuple,
                    () => func(tuple.Item1, tuple.Item2),
                    new object?[] { tuple.Item1, tuple.Item2 })));

        public static Property<(T0, T1, T2)> ForAll<T0, T1, T2>(
            IGen<T0> gen0,
            IGen<T1> gen1,
            IGen<T2> gen2,
            Func<T0, T1, T2, bool> func) => new Property<(T0, T1, T2)>(
                Gen.Zip(gen0, gen1, gen2).Select(tuple => TestFactory.Create(
                    tuple,
                    () => func(tuple.Item1, tuple.Item2, tuple.Item3),
                    new object?[] { tuple.Item1, tuple.Item2, tuple.Item3 })));

        public static Property<(T0, T1, T2, T3)> ForAll<T0, T1, T2, T3>(
            IGen<T0> gen0,
            IGen<T1> gen1,
            IGen<T2> gen2,
            IGen<T3> gen3,
            Func<T0, T1, T2, T3, bool> func) => new Property<(T0, T1, T2, T3)>(
                Gen.Zip(gen0, gen1, gen2, gen3).Select(tuple => TestFactory.Create(
                    tuple,
                    () => func(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4),
                    new object?[] { tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4 })));

        public static Property<object[]> Nullary(Action func) => Nullary(func.AsTrueFunc());

        public static Property<T0> ForAll<T0>(IGen<T0> gen0, Action<T0> action) =>
            ForAll(gen0, action.AsTrueFunc());

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
