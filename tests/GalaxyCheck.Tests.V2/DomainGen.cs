using GalaxyCheck;
using NebulaCheck;
using Gen = NebulaCheck.Gen;
using System.Linq;
using NebulaCheck.Injection.Int32;

namespace Tests.V2
{
    public static class DomainGen
    {
        public static NebulaCheck.IGen<bool> Boolean() =>
            from x in Gen.Int32().Between(0, 1).WithBias(Gen.Bias.None)
            select (x == 1);

        public class IterationsAttribute : BetweenAttribute
        {
            public IterationsAttribute() : this(1) { }

            public IterationsAttribute(int minIterations) : base(minIterations, 200) { }
        }

        public class SeedAttribute : WithBiasAttribute
        {
            public SeedAttribute() : base(Gen.Bias.None)
            {
            }
        }

        public class SizeAttribute : BetweenAttribute
        {
            public SizeAttribute() : base(0, 100) { }
        }
    }
}
