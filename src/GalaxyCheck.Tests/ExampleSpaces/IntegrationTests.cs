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
            var exampleSpace = ExampleSpaceFactory.Unfold(
                value,
                ShrinkFunc.Towards(target),
                x => x,
                IdentifyFuncs.Default<int>());

            Snapshot.Match(exampleSpace.Render(x => x.ToString()), SnapshotNameExtension.Create(value, target));
        }

        [Fact]
        public void IntegerExampleSpaceFiltered()
        {
            var exampleSpace = ExampleSpaceFactory
                .Unfold(
                    10,
                    ShrinkFunc.Towards(0),
                    x => x,
                    IdentifyFuncs.Default<int>())
                .Filter(x => x % 2 == 0);

            Snapshot.Match(exampleSpace?.Render(x => x.ToString()) ?? "");
        }
    }
}
