using FluentAssertions;
using GalaxyCheck;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Tests.GenSnapshotInitializerTests
{
    public class AboutInvalidReturnTypes
    {
#pragma warning disable xUnit1000 // Test classes must be public
        private class GenSnapshots
#pragma warning restore xUnit1000 // Test classes must be public
        {
            [GenSnapshot]
            public void GenSnapshotVoid()
            {
            }

            [GenSnapshot]
            public Task GenSnapshotTaskVoid()
            {
                return Task.CompletedTask;
            }

            [GenSnapshot]
            public ValueTask GenSnapshotValueTaskVoid()
            {
                return ValueTask.CompletedTask;
            }
        }

        [Theory]
        [InlineData(nameof(GenSnapshots.GenSnapshotVoid))]
        [InlineData(nameof(GenSnapshots.GenSnapshotTaskVoid))]
        [InlineData(nameof(GenSnapshots.GenSnapshotValueTaskVoid))]
        public void ItPassesThroughSeedsForTheIterations(string testMethodName)
        {
            var action = () => TestProxy.Initialize(typeof(GenSnapshots), testMethodName);

            action.Should().Throw<Exception>().WithMessage("Test method must return a value for snapshotting, or a task containing a value");
        }
    }
}
