using GalaxyCheck;
using NebulaCheck;
using Gen = NebulaCheck.Gen;
using System.Linq;

namespace Tests.V2
{
    public static class DomainGen
    {
        public static NebulaCheck.IGen<int> Iterations(int? minIterations = null) => Gen.Int32().Between(minIterations ?? 1, 200);

        public static NebulaCheck.IGen<int> Seed() => Gen.Int32().WithBias(Gen.Bias.None).NoShrink();

        public static NebulaCheck.IGen<int> Size() => Gen.Int32().Between(0, 100);

        public static NebulaCheck.IGen<bool> Boolean() =>
            from x in Gen.Int32().Between(0, 1).WithBias(Gen.Bias.None)
            select (x == 1);
    }
}
