﻿using FluentAssertions;
using GalaxyCheck.Gens.Parameters;
using GalaxyCheck.Gens.Parameters.Internal;
using NebulaCheck;
using Snapshooter;
using Snapshooter.Xunit;
using System.Linq;
using Xunit;

namespace Tests.V2.ImplementationTests.SizingTests
{
    public class AboutSizingToBounds
    {
        [Property]
        public IGen<Test> IfSizeIsZero_ItProducesTheOriginBounds() =>
            from min in Gen.Int32()
            from max in Gen.Int32().GreaterThanEqual(min)
            from origin in Gen.Int32().Between(min, max)
            select Property.ForThese(() =>
            {
                var sizeToBounds = SizingToBounds.Exponential(min, max, origin);

                var bounds = sizeToBounds(new Size(0));

                bounds.Should().Be((origin, origin));
            });

        [Property]
        public IGen<Test> IfSizeIsOneHundred_ItProducesTheExtremeBounds() =>
            from min in Gen.Int32()
            from max in Gen.Int32().GreaterThanEqual(min)
            from origin in Gen.Int32().Between(min, max)
            select Property.ForThese(() =>
            {
                var sizeToBounds = SizingToBounds.Exponential(min, max, origin);

                var bounds = sizeToBounds(new Size(100));

                bounds.Should().Be((min, max));
            });

        [Theory]
        [InlineData(int.MinValue, int.MaxValue, 0)]
        [InlineData(int.MinValue / 2, int.MaxValue, 0)]
        [InlineData(int.MinValue, int.MaxValue / 2, 0)]
        [InlineData(int.MinValue, int.MaxValue, int.MinValue)]
        [InlineData(int.MinValue, int.MaxValue, int.MaxValue)]
        public void Snapshots(int min, int max, int origin)
        {
            var sizeToBounds = SizingToBounds.Exponential(min, max, origin);

            var bounds = Enumerable
                .Range(0, 101)
                .Select(value => new Size(value))
                .Select(size => sizeToBounds(size))
                .Select(bounds => $"({bounds.min}, {bounds.max})")
                .ToList();

            Snapshot.Match(bounds, SnapshotNameExtension.Create($"min={min};max={max};origin={origin}"));
        }
    }
}
