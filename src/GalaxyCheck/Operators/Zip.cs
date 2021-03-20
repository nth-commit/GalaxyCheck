using GalaxyCheck.Internal;
using System.Collections.Generic;
using System.Linq;

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
                Select(gen0, gen1, gen2, gen3, (x0, x1, x2, x3) => (x0, x1, x2, x3));

        public static IGen<IEnumerable<T>> Zip<T>(IEnumerable<IGen<T>> gens)
        {
            var (head, tail) = gens;

            if (head == null)
            {
                return Constant(Enumerable.Empty<T>());
            }

            return from x in head
                   from xs in Zip(tail)
                   select Enumerable.Concat(new[] { x }, xs);
        }

        public static IGen<T[]> Zip<T>(IGen<T>[] gens) => Zip(gens.AsEnumerable()).Select(xs => xs.ToArray());
    }
}
