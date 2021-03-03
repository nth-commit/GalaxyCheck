using NebulaCheck;
using NebulaCheck.Gens;
using NebulaCheck.Injection.Int32;

namespace Tests.V2
{
    public static class DomainGenAttributes
    {
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
