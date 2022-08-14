using FluentAssertions;
using GalaxyCheck;
using GalaxyCheck.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Tests.GenSnapshotInitializerTests
{
    public class AboutAssertSnapshotMatches
    {
#pragma warning disable xUnit1000 // Test classes must be public
        private class GenSnapshots
#pragma warning restore xUnit1000 // Test classes must be public
        {
            [GenSnapshot]
            public object GenSnapshot() => new { };
        }

        [Fact]
        public void ItPassesThroughAssertSnapshotMatches()
        {
            Func<ISnapshot, Task> assertSnapshotMatches = (_) => Task.CompletedTask;

            var result = TestProxy.Initialize(
                typeof(GenSnapshots),
                nameof(GenSnapshots.GenSnapshot),
                configure: config => config.AssertSnapshotMatches = assertSnapshotMatches);

            result.AssertSnapshotMatches.Should().Be(assertSnapshotMatches);
        }

        [Fact]
        public void ItThrowsIfAssertSnapshotMatchesIsNull()
        {
            var action = () => TestProxy.Initialize(
                typeof(GenSnapshots),
                nameof(GenSnapshots.GenSnapshot),
                configure: config => config.AssertSnapshotMatches = null);

            action.Should().Throw<Exception>().WithMessage("Configuration value \"AssertSnapshotMatches\" needs to be set in order to use gen snapshots");
        }
    }
}
