using GalaxyCheck;
using NebulaCheck;
using System;
using System.Linq;

namespace Tests.V2
{
    public static class DomainGen
    {
        public static NebulaCheck.IGen<T> Element<T>(params T[] elements)
        {
            if (elements.Any() == false)
            {
                return NebulaCheck.Gen.Advanced.Error<T>("DomainGen.Element", "Input gens was empty");
            }

            return NebulaCheck.Gen.Int32()
                .Between(0, elements.Length - 1)
                .Select(i => elements[i]);
        }

        public static NebulaCheck.IGen<int> Seed() => NebulaCheck.Gen.Int32().WithBias(NebulaCheck.Gen.Bias.None).NoShrink();

        public static NebulaCheck.IGen<int> Size(bool allowChaos = true) => allowChaos
            ? NebulaCheck.Gen.Int32().Between(0, 100)
            : NebulaCheck.Gen.Int32().Between(0, 50);

        public static NebulaCheck.IGen<int> Iterations(bool allowZero = true) => NebulaCheck.Gen.Int32().Between(allowZero ? 0 : 1, 200);

        public static NebulaCheck.IGen<GalaxyCheck.IGen<object>> Gen() =>
            NebulaCheck.Gen.Choose<GalaxyCheck.IGen>(
                NebulaCheck.Gen.Constant(GalaxyCheck.Gen.Int32().Between(-1000, 1000)),
                NebulaCheck.Gen.Constant(GalaxyCheck.Gen.Int32().Between(0, 1).WithBias(GalaxyCheck.Gen.Bias.None))
            ).Select(gen => gen.Cast<object>());

        public static NebulaCheck.IGen<(T, T)> Two<T>(this NebulaCheck.IGen<T> gen) => NebulaCheck.Gen.Zip(gen, gen);

        public static NebulaCheck.IGen<(T, T, T)> Three<T>(this NebulaCheck.IGen<T> gen) => NebulaCheck.Gen.Zip(gen, gen, gen);

        public static NebulaCheck.IGen<(T, T, T, T)> Four<T>(this NebulaCheck.IGen<T> gen) => NebulaCheck.Gen.Zip(gen, gen, gen, gen    );

        public static NebulaCheck.IGen<bool> Boolean() =>   
            from x in NebulaCheck.Gen.Int32().Between(0, 1).WithBias(NebulaCheck.Gen.Bias.None)
            select (x == 1);

        public static NebulaCheck.IGen<object> Any() => NebulaCheck.Gen.Choose(
            NebulaCheck.Gen.Int32().Cast<object>(),
            Boolean().Cast<object>());

        public static NebulaCheck.Gens.IListGen<object> AnyList() => Any().ListOf();

        public static NebulaCheck.IGen<Action> Action() =>
            Boolean().Select<bool, Action>(b => () => IfTrueThenThrow(b));

        public static NebulaCheck.IGen<Action<T0>> Action<T0>() =>
            Boolean().Select<bool, Action<T0>>(b => (_) => IfTrueThenThrow(b));

        public static NebulaCheck.IGen<Action<T0, T1>> Action<T0, T1>() =>
            Boolean().Select<bool, Action<T0, T1>>(b => (_, __) => IfTrueThenThrow(b));

        public static NebulaCheck.IGen<Action<T0, T1, T2>> Action<T0, T1, T2>() =>
            Boolean().Select<bool, Action<T0, T1, T2>>(b => (_, __, ___) => IfTrueThenThrow(b));

        public static NebulaCheck.IGen<Action<T0, T1, T2, T3>> Action<T0, T1, T2, T3>() =>
            Boolean().Select<bool, Action<T0, T1, T2, T3>>(b => (_, __, ___, ____) => IfTrueThenThrow(b));

        public static NebulaCheck.IGen<Func<T, bool>> Predicate<T>() => NebulaCheck.Gen.Function<T, bool>(Boolean());

        private static void IfTrueThenThrow(bool b)
        {
            if (b == true)
            {
                throw new Exception("Action threw");
            }
        }
    }
}
