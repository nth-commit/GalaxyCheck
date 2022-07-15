using NebulaCheck;
using GalaxyCheck;
using System;
using System.Linq;
using FluentAssertions;
using Xunit;
using static Tests.V2.DomainGenAttributes;

namespace Tests.V2.PropertyTests.AboutPreconditions
{
    public class AboutPreconditions
    {
        [Property(Iterations = 10)]
        public void WhenThePredicateOnlyPassesForLargerSizesItStillProducesValues([Seed] int seed, [Size] int size)
        {
            var property = GalaxyCheck.Gen
                .Create(parameters => (parameters.Size.Value, parameters))
                .ForAll(size => GalaxyCheck.Property.Precondition(size >= 50));

            Action test = () => property.Check(seed: seed, size: size);

            test.Should().NotThrow<GalaxyCheck.Exceptions.GenExhaustionException>();
        }

        [Fact]
        public void IfThePreconditionPasses_ThenTheEquivalentAssertionPasses()
        {
            Func<int, bool> pred = x => x % 2 == 0;

            var result = GalaxyCheck.Gen
                .Int32()
                .ForAll(x =>
                {
                    GalaxyCheck.Property.Precondition(pred(x));

                    pred(x).Should().BeTrue();
                })
                .Check(seed: 0);

            result.Counterexample.Should().BeNull();
        }

        [Fact]
        public void IfThePreconditionPasses_WhenThePropertyIsSampled_AllTheValuesPassThePrecondition()
        {
            Func<int, bool> pred = x => x % 2 == 0;

            var result = GalaxyCheck.Gen
                .Int32()
                .ForAll(x =>
                {
                    GalaxyCheck.Property.Precondition(pred(x));

                    pred(x).Should().BeTrue();
                })
                .Sample(seed: 0);

            result.Should().OnlyContain(x => pred(x));
        }
    }
}
