using NebulaCheck;
using NebulaCheck.Gens.Injection.Int32;

namespace Tests.V2
{
    public static class DomainGenAttributes
    {
        public class IterationsAttribute : GenProviderAttribute
        {
            public int MinIterations { get; }

            public IterationsAttribute() : this(1) { }

            public IterationsAttribute(int minIterations)
            {
                MinIterations = minIterations;
            }

            public override IGen Get => Gen.Int32().Between(MinIterations, 200);
        }

        public class SeedAttribute : GenProviderAttribute
        {
            public override IGen Get => DomainGen.Seed();
        }

        public class SizeAttribute : GenProviderAttribute
        {
            public override IGen Get => DomainGen.Size();
        }
    }
}
