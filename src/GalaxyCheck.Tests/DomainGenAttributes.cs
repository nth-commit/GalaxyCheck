using NebulaCheck;

namespace Tests.V2
{
    public static class DomainGenAttributes
    {
        public class IterationsAttribute : GenAttribute
        {
            public int MinIterations { get; }

            public int MaxIterations { get; }

            public IterationsAttribute(int minIterations = 1, int maxIterations = 200)
            {
                MinIterations = minIterations;
                MaxIterations = maxIterations;
            }

            public override IGen Value => Gen.Int32().Between(MinIterations, MaxIterations);
        }

        public class SeedAttribute : GenAttribute
        {
            public override IGen Value => DomainGen.Seed();
        }

        public class SizeAttribute : GenAttribute
        {
            public bool AllowChaos { get; set; } = true;

            public override IGen Value => DomainGen.Size(AllowChaos);
        }

        public class PickGenAttribute : GenAttribute
        {
            public override IGen Value => DomainGen.Gen();
        }
    }
}
