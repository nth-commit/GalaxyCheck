using FluentAssertions;
using GalaxyCheck;
using GalaxyCheck.Runners;
using Snapshooter.Xunit;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using Xunit;

namespace Tests.V2.RunnerTests.AssertTests
{
    public class Snapshots
    {
        [Fact]
        public void Snapshot_AssertFailure_IntLessThanEquals()
        {
            // TODO: Cross-platform consistent replay encoding
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var property = Gen.Int32().ForAll(x => x < 1000);

                Action action = () => property.Assert(seed: 0);

                action.Should().Throw<PropertyFailedException>().Which.Message.MatchSnapshot();
            }
        }

        [Fact]
        public void Snapshot_AssertFailure_IntLessThanEquals_BinaryProperty()
        {
            // TODO: Cross-platform consistent replay encoding
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var property = Property.FromLinq(
                from x in Gen.Int32()
                from y in Gen.Int32().GreaterThan(x)
                select Property.ForThese(() => x < 1000));

                Action action = () => property.Assert(seed: 0);

                action.Should().Throw<PropertyFailedException>().Which.Message.MatchSnapshot();
            }
        }
    }
}
