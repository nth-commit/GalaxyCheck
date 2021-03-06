﻿using FluentAssertions;
using GalaxyCheck.ExampleSpaces;
using NebulaCheck;
using System.Linq;

namespace Tests.V2.ExampleSpaces
{
    public class AboutInt64ExampleSpace
    {
        [Property]
        public IGen<Test> ItShrinksToTheOrigin() =>
            from origin in Gen.Int64()
            from root in Gen.Int64()
            select Property.ForThese(() =>
            {
                var exampleSpace = ExampleSpaceFactory.Int64(root, origin, long.MinValue, long.MaxValue);

                var path = exampleSpace
                    .ExploreCounterexamples(x => false)
                    .Where(x => x.IsCounterexample())
                    .Select(x => x.Value());

                path.Should().EndWith(origin);
            });

        [Property]
        public IGen<Test> ForAPositiveRoot_ItShrinksToTheLocalMinimum() =>
            from localMin in Gen.Int64().GreaterThanEqual(0)
            from root in Gen.Int64().GreaterThanEqual(localMin)
            select Property.ForThese(() =>
            {
                var exampleSpace = ExampleSpaceFactory.Int64(root, 0, long.MinValue, long.MaxValue);

                var path = exampleSpace
                    .ExploreCounterexamples(x => x < localMin)
                    .Where(x => x.IsCounterexample())
                    .Select(x => x.Value());

                path.Should().EndWith(localMin);
            });

        [Property]
        public IGen<Test> ForANegativeRoot_ItShrinksToTheLocalMaximum() =>
            from localMax in Gen.Int64().LessThan(0).GreaterThan(int.MinValue)
            from root in Gen.Int64().LessThan(localMax)
            select Property.ForThese(() =>
            {
                var exampleSpace = ExampleSpaceFactory.Int64(root, 0, long.MinValue, long.MaxValue);

                var path = exampleSpace
                    .ExploreCounterexamples(x => x > localMax)
                    .Where(x => x.IsCounterexample())
                    .Select(x => x.Value());

                path.Last().Should().Be(localMax);
            });
    }
}
