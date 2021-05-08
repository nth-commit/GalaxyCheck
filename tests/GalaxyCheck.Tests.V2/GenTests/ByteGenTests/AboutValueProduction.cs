using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using System.Collections.Generic;
using System.Linq;
using Gen = NebulaCheck.Gen;
using Property = NebulaCheck.Property;
using Test = NebulaCheck.Test;

namespace Tests.V2.GenTests.ByteGenTests
{
    public class AboutValueProduction
    {
        [Property]
        public NebulaCheck.IGen<Test> ItProducesValuesLikeInt32() =>
            from min in Gen.Int32().Between(0, byte.MaxValue)
            from max in Gen.Int32().Between(min, byte.MaxValue)
            from origin in Gen.Int32().Between(min, max)
            from bias in DomainGen.Bias()
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                List<object> SampleTraversal(GalaxyCheck.IGen gen) =>
                    gen.Cast<object>().SampleOneTraversal(seed, size);

                var gen0 = GalaxyCheck.Gen.Byte().GreaterThanEqual((byte)min).LessThanEqual((byte)max).ShrinkTowards((byte)origin).WithBias(bias);
                var gen1 = GalaxyCheck.Gen.Int32().Between(min, max).ShrinkTowards(origin).WithBias(bias);

                var sample0 = SampleTraversal(gen0);
                var sample1 = SampleTraversal(gen1);

                sample0.Should().BeEquivalentTo(sample1);
            });

        [Property]
        public NebulaCheck.IGen<Test> ItProducesValuesGreaterThanOrEqualMinimum() =>
            from min in Gen.Int32().Between(0, byte.MaxValue)
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen.Byte().GreaterThanEqual((byte)min);

                var sample = gen.SampleOne(seed: seed, size: size);

                sample.Should().BeGreaterOrEqualTo((byte)min);
            });

        [Property]
        public NebulaCheck.IGen<Test> ItProducesValuesLessThanOrEqualMaximum() =>
            from max in Gen.Int32().Between(0, byte.MaxValue)
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen.Byte().LessThanEqual((byte)max);

                var sample = gen.SampleOne(seed: seed, size: size);

                sample.Should().BeLessOrEqualTo((byte)max);
            });
    }
}
