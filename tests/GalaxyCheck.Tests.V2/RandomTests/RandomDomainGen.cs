using NebulaCheck;
using System.Linq;

namespace Tests.V2.RandomTests
{
    public static class RandomDomainGen
    {
        public static IGen<GalaxyCheck.IRng> Rng() =>
            from seed in Gen.Int32()
            from iterations in Gen.Int32().Between(0, 10)
            select Enumerable.Range(0, iterations).Aggregate(
                GalaxyCheck.Internal.Random.Rng.Create(seed),
                (rng, _) => rng.Next());
    }
}
