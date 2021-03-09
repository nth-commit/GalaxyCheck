﻿using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using NebulaCheck.Xunit;
using System.Linq;
using Gen = NebulaCheck.Gen;
using Property = NebulaCheck.Property;
using Test = NebulaCheck.Test;

namespace Tests.V2.GenTests.Int32GenTests
{
    public class AboutConstraints
    {
        [Property]
        public NebulaCheck.IGen<Test> ItProducesValuesGreaterThanOrEqualMinimum() =>
            from min in Gen.Int32()
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen.Int32().GreaterThanEqual(min);

                var exampleSpace = gen.Advanced.SampleOneExampleSpace(seed: seed, size: size);

                exampleSpace.Traverse().Take(100).Should().OnlyContain(value => value >= min);
            });

        [Property]
        public NebulaCheck.IGen<Test> ItProducesValuesLessThanOrEqualMaximum() =>
            from max in Gen.Int32()
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen.Int32().LessThanEqual(max);

                var exampleSpace = gen.Advanced.SampleOneExampleSpace(seed: seed, size: size);

                exampleSpace.Traverse().Take(100).Should().OnlyContain(value => value <= max);
            });
    }
}
