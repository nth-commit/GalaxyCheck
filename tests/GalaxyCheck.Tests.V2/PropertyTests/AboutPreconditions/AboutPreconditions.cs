using NebulaCheck;
using GalaxyCheck;
using System;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Xunit;
using static Tests.V2.DomainGenAttributes;

namespace Tests.V2.PropertyTests.AboutPreconditions
{
    public class AboutPreconditions
    {
        [Property(Iterations = 1, Skip = "Flakey, soon-to-be-deleted test")]

        public void WhenThePredicateOnlyPassesForLargerSizesItStillProducesValues([Seed] int seed, [Size] int size)
        {
            var property = GalaxyCheck.Gen
                .Create(parameters => (parameters.Size.Value, parameters))
                .ForAll(size => GalaxyCheck.Property.Precondition(size >= 50));

            Action test = () => property.Check(seed: seed, size: size);

            test.Should().NotThrow<GalaxyCheck.Exceptions.GenExhaustionException>();
        }

        [Fact(Skip = "Regression")]
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

        private GalaxyCheck.Property NestedPrecondition(int x)
        {
            Func<int, bool> pred = x => x % 2 == 0;

            GalaxyCheck.Property.Precondition(pred(x));

            return GalaxyCheck.Gen.Int32().ForAll(_ => pred(x).Should().BeTrue());
        }

        [Fact]
        public void ForNestedProperties_IfThePreconditionPasses_ThenTheEquivalentAssertPasses()
        {
            var property = GalaxyCheck.Property.Reflect(GetMethod(nameof(NestedPrecondition)), this);

            var result = property.Check(seed: 0);

            result.Counterexample.Should().BeNull();
        }

        private static MethodInfo GetMethod(string name)
        {
            var methodInfo = typeof(AboutPreconditions).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance);

            if (methodInfo == null)
            {
                throw new Exception("Unable to locate method");
            }

            return methodInfo;
        }
    }
}
