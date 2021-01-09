using Snapshooter;
using Snapshooter.Xunit;
using Xunit;
using GC = GalaxyCheck;
using GalaxyCheck.ExampleSpaces;

namespace Tests.ExampleSpaces
{
    public class IntegrationTests
    {
        [Theory]
        [InlineData(1, 0)]
        [InlineData(2, 0)]
        [InlineData(5, 0)]
        [InlineData(10, 0)]
        public void IntegerExampleSpace(int value, int target)
        {
            var exampleSpace = GC.ExampleSpaces.ExampleSpace.Unfold(
                value,
                GC.ExampleSpaces.ShrinkFunc.Towards(target),
                x => x);

            Snapshot.Match(exampleSpace.Render(x => x.ToString()), SnapshotNameExtension.Create(value, target));
        }

        [Fact]
        public void IntegerExampleSpaceFiltered()
        {
            var exampleSpace = GC.ExampleSpaces.ExampleSpace
                .Unfold(10, GC.ExampleSpaces.ShrinkFunc.Towards(0), x => x)
                .Filter(x => x % 2 == 0);

            Snapshot.Match(exampleSpace?.Render(x => x.ToString()) ?? "");
        }
    }
}
