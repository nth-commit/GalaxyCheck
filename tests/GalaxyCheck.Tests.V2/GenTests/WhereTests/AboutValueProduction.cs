using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using System;
using System.Linq;
using static Tests.V2.DomainGenAttributes;

namespace Tests.V2.GenTests.WhereTests
{
    public class AboutValueProduction
    {
        [Property]
        public void ItProducesValuesThatPassThePredicate([Seed] int seed, [Size] int size)
        {
            Func<int, bool> pred = x => x % 2 == 0;
            var gen = GalaxyCheck.Gen.Int32().Where(pred);

            var sample = gen.SampleOneTraversal(seed: seed, size: size);

            sample.Should().OnlyContain(x => pred(x));
        }

        [Property]
        public void WhenThePredicateOnlyPassesForLargerSizesItStillProducesValues([Seed] int seed, [Size] int size)
        {
            var gen = GalaxyCheck.Gen.Create(parameters => (parameters.Size.Value, parameters)).Where(size => size > 50);

            Action test = () => gen.SampleOneTraversal(seed: seed, size: size);

            test.Should().NotThrow<GalaxyCheck.Exceptions.GenExhaustionException>();
        }
    }
}
