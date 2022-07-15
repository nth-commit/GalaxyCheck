using GalaxyCheck.Properties;
using System;
using System.Threading.Tasks;

namespace GalaxyCheck
{
    public partial class Property
    {
        public static AsyncProperty<object[]> NullaryAsync(Func<Task<bool>> func) => new AsyncProperty<object[]>(
            Gen.Constant(new object[] { }).Select(x => TestFactory.Create<object[]>(
                x,
                func,
                x)));

        public static AsyncProperty<T0> ForAllAsync<T0>(IGen<T0> gen0, Func<T0, Task<bool>> func) =>
            new AsyncProperty<T0>(
                gen0.Select(x => TestFactory.Create(
                    x,
                    () => func(x),
                    new object?[] { x })));

        public static AsyncProperty<(T0, T1)> ForAllAsync<T0, T1>(
            IGen<T0> gen0,
            IGen<T1> gen1,
            Func<T0, T1, Task<bool>> func) => new AsyncProperty<(T0, T1)>(
                Gen.Zip(gen0, gen1).Select(tuple => TestFactory.Create(
                    tuple,
                    () => func(tuple.Item1, tuple.Item2),
                    new object?[] { tuple.Item1, tuple.Item2 })));

        public static AsyncProperty<(T0, T1, T2)> ForAllAsync<T0, T1, T2>(
            IGen<T0> gen0,
            IGen<T1> gen1,
            IGen<T2> gen2,
            Func<T0, T1, T2, Task<bool>> func) => new AsyncProperty<(T0, T1, T2)>(
                Gen.Zip(gen0, gen1, gen2).Select(tuple => TestFactory.Create(
                    tuple,
                    () => func(tuple.Item1, tuple.Item2, tuple.Item3),
                    new object?[] { tuple.Item1, tuple.Item2, tuple.Item3 })));

        public static AsyncProperty<(T0, T1, T2, T3)> ForAllAsync<T0, T1, T2, T3>(
            IGen<T0> gen0,
            IGen<T1> gen1,
            IGen<T2> gen2,
            IGen<T3> gen3,
            Func<T0, T1, T2, T3, Task<bool>> func) => new AsyncProperty<(T0, T1, T2, T3)>(
                Gen.Zip(gen0, gen1, gen2, gen3).Select(tuple => TestFactory.Create(
                    tuple,
                    () => func(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4),
                    new object?[] { tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4 })));

        public static AsyncProperty<object[]> NullaryAsync(Func<Task> func) => NullaryAsync(func.AsTrueFunc());

        public static AsyncProperty<T0> ForAllAsync<T0>(IGen<T0> gen0, Func<T0, Task> action) =>
            ForAllAsync(gen0, action.AsTrueFunc());

        public static AsyncProperty<(T0, T1)> ForAllAsync<T0, T1>(
            IGen<T0> gen0,
            IGen<T1> gen1,
            Func<T0, T1, Task> action) => ForAllAsync(gen0, gen1, action.AsTrueFunc());

        public static AsyncProperty<(T0, T1, T2)> ForAllAsync<T0, T1, T2>(
            IGen<T0> gen0,
            IGen<T1> gen1,
            IGen<T2> gen2,
            Func<T0, T1, T2, Task> action) => ForAllAsync(gen0, gen1, gen2, action.AsTrueFunc());

        public static AsyncProperty<(T0, T1, T2, T3)> ForAllAsync<T0, T1, T2, T3>(
            IGen<T0> gen0,
            IGen<T1> gen1,
            IGen<T2> gen2,
            IGen<T3> gen3,
            Func<T0, T1, T2, T3, Task> action) => ForAllAsync(gen0, gen1, gen2, gen3, action.AsTrueFunc());
    }
}
