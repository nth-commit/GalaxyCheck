using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using System;
using System.Linq;
using static Tests.V2.DomainGenAttributes;
using Property = NebulaCheck.Property;
using Test = NebulaCheck.Test;

namespace Tests.V2.GenTests.DateTimeGenTests
{
    public class AboutConstraints
    {
        [Property]
        public void ItProducesValuesFromTheGivenDate(
            [Seed] int seed,
            [Size] int size,
            [DateTime] DateTime dateTime)
        {
            var gen = GalaxyCheck.Gen.DateTime().From(dateTime);

            var sample = gen.SampleOneTraversal(seed: seed, size: size);

            sample.Should().OnlyContain(dt => dt >= dateTime);
        }

        [Property]
        public void ItProducesValuesToTheGivenDate(
            [Seed] int seed,
            [Size] int size,
            [DateTime] DateTime dateTime)
        {
            var gen = GalaxyCheck.Gen.DateTime().Until(dateTime);

            var sample = gen.SampleOneTraversal(seed: seed, size: size);

            sample.Should().OnlyContain(dt => dt <= dateTime);
        }

        [Property]
        public NebulaCheck.IGen<Test> ItProducesValuesWithinTheGivenDates() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            from minDateTime in DomainGen.DateTime()
            from maxDateTime in DomainGen.DateTime(minDateTime: minDateTime)
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen.DateTime().From(minDateTime).Until(maxDateTime);

                var sample = gen.SampleOneTraversal(seed: seed, size: size);

                sample.Should().OnlyContain(dt => minDateTime <= dt && dt <= maxDateTime);
            });

    }
}
