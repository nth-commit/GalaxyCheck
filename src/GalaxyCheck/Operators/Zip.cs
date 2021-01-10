using System;
using System.Collections.Generic;
using System.Text;

namespace GalaxyCheck
{
    public static partial class Gen
    {
        public static IGen<(T0, T1)> Zip<T0, T1>(
            IGen<T0> gen0,
            IGen<T1> gen1) =>
                Select(gen0, gen1, (x0, x1) => (x0, x1));

        public static IGen<(T0, T1, T2)> Zip<T0, T1, T2>(
            IGen<T0> gen0,
            IGen<T1> gen1,
            IGen<T2> gen2) =>
                Select(gen0, gen1, gen2, (x0, x1, x2) => (x0, x1, x2));

        public static IGen<(T0, T1, T2, T3)> Zip<T0, T1, T2, T3>(
            IGen<T0> gen0,
            IGen<T1> gen1,
            IGen<T2> gen2,
            IGen<T3> gen3) =>
                Select(gen0, gen1, gen2, gen3,(x0, x1, x2, x3) => (x0, x1, x2, x3));
    }
}
