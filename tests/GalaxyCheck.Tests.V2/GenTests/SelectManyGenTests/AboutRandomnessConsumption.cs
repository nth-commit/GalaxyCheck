using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using System.Linq;
using static Tests.V2.DomainGenAttributes;

namespace Tests.V2.GenTests.SelectManyGenTests
{
    public class AboutRandomnessConsumption
    {
        [Property]
        public void ItConsumesAsMuchRandomnessAsTheLeftAndRightGeneratorIndividually(
            [Seed] int seed,
            [Size(AllowChaos = false)] int size)
        {
            int MeasureRandomnessConsumption(GalaxyCheck.IGen gen) => gen
                .Cast<object>()
                .Advanced.SampleOneWithMetrics(seed: seed, size: size)
                .RandomnessConsumption;

            var genLeft = GalaxyCheck.Gen.Int32();
            var genRight = GalaxyCheck.Gen.Int32();
            var gen = genLeft.SelectMany(_ => genRight);

            MeasureRandomnessConsumption(gen).Should()
                .Be(MeasureRandomnessConsumption(genLeft) + MeasureRandomnessConsumption(genRight));
        }
    }
}
