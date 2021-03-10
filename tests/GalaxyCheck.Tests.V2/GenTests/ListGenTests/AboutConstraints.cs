using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using NebulaCheck.Xunit;
using System.Linq;
using Gen = NebulaCheck.Gen;
using Property = NebulaCheck.Property;
using Test = NebulaCheck.Test;

namespace Tests.V2.GenTests.ListGenTests
{
    public class AboutConstraints
    {
        [Property]
        public NebulaCheck.IGen<Test> ItProducesValuesWithLengthGreaterThanOrEqualMinimumLength() =>
            from elementGen in DomainGen.Gen()
            from minLength in Gen.Int32().Between(0, 10)
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var gen = elementGen.ListOf().OfMinimumLength(minLength);

                var exampleSpace = gen.Advanced.SampleOneExampleSpace(seed: seed, size: size);

                exampleSpace.Traverse().Take(100).Should().OnlyContain(value => value.Count >= minLength);
            });

        [Property]
        public NebulaCheck.IGen<Test> ItProducesValuesWithLengthLessThanOrEqualMaximumLength() =>
            from elementGen in DomainGen.Gen()
            from maxLength in Gen.Int32().Between(0, 10)
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var gen = elementGen.ListOf().OfMaximumLength(maxLength);

                var exampleSpace = gen.Advanced.SampleOneExampleSpace(seed: seed, size: size);

                exampleSpace.Traverse().Take(100).Should().OnlyContain(value => value.Count <= maxLength);
            });

        [Property]
        public NebulaCheck.IGen<Test> ItProducesValuesWithOfTheSpecificLength() =>
            from elementGen in DomainGen.Gen()
            from length in Gen.Int32().Between(0, 20)
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var gen = elementGen.ListOf().OfLength(length);

                var exampleSpace = gen.Advanced.SampleOneExampleSpace(seed: seed, size: size);

                exampleSpace.Traverse().Take(100).Should().OnlyContain(value => value.Count == length);
            });
    }
}
