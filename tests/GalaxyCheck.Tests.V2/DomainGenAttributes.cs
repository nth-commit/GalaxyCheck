using NebulaCheck;

namespace Tests.V2
{
    public static class DomainGenAttributes
    {
        public class IterationsAttribute : GenAttribute
        {
            public int MinIterations { get; }

            public IterationsAttribute() : this(1) { }

            public IterationsAttribute(int minIterations)
            {
                MinIterations = minIterations;
            }

            public override IGen Get => Gen.Int32().Between(MinIterations, 200);
        }

        public class SeedAttribute : GenAttribute
        {
            public override IGen Get => DomainGen.Seed();
        }

        public class SizeAttribute : GenAttribute
        {
            public override IGen Get => DomainGen.Size();
        }
    }
}
