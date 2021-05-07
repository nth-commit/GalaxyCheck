using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using System.Collections.Generic;
using System.Linq;
using static Tests.V2.DomainGenAttributes;

namespace Tests.V2.GenTests.ReflectedGenTests
{
    public class AboutGeneratingFromASubType
    {
        [Property]
        public void ItGeneratesValuesWithTheRegisteredSubTypeGenerator([Seed] int seed, [Size] int size)
        {
            List<object> SampleTraversal(GalaxyCheck.IGen<object> gen) =>
                AboutGeneratingFromASubType.SampleTraversal(gen, seed, size);

            var gen0 = GalaxyCheck.Gen.String();
            var gen1 = GalaxyCheck.Gen
                .Factory()
                .RegisterType(typeof(object), gen0)
                .Create<object>();

            var sample0 = SampleTraversal(gen0);
            var sample1 = SampleTraversal(gen1);

            sample0.Should().BeEquivalentTo(sample1);
        }

        private static List<T> SampleTraversal<T>(GalaxyCheck.IGen<T> gen, int seed, int size) => gen.Advanced
            .SampleOneExampleSpace(seed: seed, size: size)
            .Traverse()
            .Take(100)
            .ToList();
    }
}
