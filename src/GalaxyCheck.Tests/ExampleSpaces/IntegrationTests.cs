using Snapshooter;
using Snapshooter.Xunit;
using Xunit;
using GC = GalaxyCheck;
using GalaxyCheck.Internal.ExampleSpaces;

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
            var exampleSpace = GC.Internal.ExampleSpaces.ExampleSpace.Unfold(
                value,
                GC.Internal.ExampleSpaces.ShrinkFunc.Towards(target),
                x => x,
                IdentifyFuncs.Default<int>());

            Snapshot.Match(exampleSpace.Render(x => x.ToString()), SnapshotNameExtension.Create(value, target));
        }

        [Fact]
        public void IntegerExampleSpaceFiltered()
        {
            var exampleSpace = GC.Internal.ExampleSpaces.ExampleSpace
                .Unfold(
                    10,
                    GC.Internal.ExampleSpaces.ShrinkFunc.Towards(0),
                    x => x,
                    IdentifyFuncs.Default<int>())
                .Filter(x => x % 2 == 0);

            Snapshot.Match(exampleSpace?.Render(x => x.ToString()) ?? "");
        }
    }
}
