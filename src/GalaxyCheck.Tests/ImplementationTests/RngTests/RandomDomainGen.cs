using NebulaCheck;
using System.Linq;

namespace Tests.V2.ImplementationTests.RngTests
{
    public static class RandomDomainGen
    {
        public static IGen<GalaxyCheck.Gens.Parameters.IRng> Rng() =>
            from seed in Gen.Int32()
            from iterations in Gen.Int32().Between(0, 10)
            select Enumerable.Range(0, iterations).Aggregate(
                GalaxyCheck.Gens.Parameters.Internal.Rng.Create(seed),
                (rng, _) => rng.Next());
    }
}
