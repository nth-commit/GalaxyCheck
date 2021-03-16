using FluentAssertions;
using GalaxyCheck;
using GalaxyCheck.Internal.ExampleSpaces;
using NebulaCheck;
using NebulaCheck.Xunit;
using System;
using System.Linq;
using Gen = NebulaCheck.Gen;
using Property = NebulaCheck.Property;
using Test = NebulaCheck.Test;

namespace Tests.V2.RunnerTests.MinimumTests
{
    public class AboutMinimumWithMetrics
    {
        [Property]
        public static NebulaCheck.IGen<Test> ItReportsTheNumberOfTimesTheGenCouldShrink() =>
            from gen in DomainGen.Gen()
            from numberOfShrinks in Gen.Int32().Between(0, 100)
            from seed in DomainGen.Seed()
            select Property.ForThese(() =>
            {
                var minimum = gen
                    .Advanced.Unfold(x => UnfoldToNumberOfShrinks(x, numberOfShrinks))
                    .Advanced.MinimumWithMetrics(seed: seed);

                minimum.Shrinks.Should().Be(numberOfShrinks);
            });

        private static IExampleSpace<T> UnfoldToNumberOfShrinks<T>(T value, int numberOfShrinks) =>
            ExampleSpaceFactory.Unfold(
                value,
                new ShrinkNumberOfTimes<T>(numberOfShrinks),
                MeasureFunc.Unmeasured<T>(),
                (_) => RandomId());

        private class ShrinkNumberOfTimes<T> : ContextualShrinker<T, int>
        {
            private readonly int _n;

            public ShrinkNumberOfTimes(int n)
            {
                _n = n;
            }

            public int RootContext => 0;

            public ContextualShrinkFunc<T, int> ContextualShrink => (value, n) =>
            {
                if (n == _n)
                {
                    return (n, Enumerable.Empty<T>());
                }

                return (n + 1, new[] { value });
            };

            public NextContextFunc<T, int> ContextualTraverse => (_, n) => n;
        }

        private static ExampleId RandomId() => ExampleId.Primitive(Guid.NewGuid().GetHashCode());
    }
}
