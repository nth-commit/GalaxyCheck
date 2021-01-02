using Snapshooter;
using Snapshooter.Xunit;
using Xunit;
using ES = GalaxyCheck.ExampleSpaces;

namespace GalaxyCheck.Tests.ExampleSpaces
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
            var exampleSpace = ES.ExampleSpace.Unfold(value, ES.ShrinkFunc.Towards(target), x => x);

            Snapshot.Match(exampleSpace.Render(x => x.ToString()), SnapshotNameExtension.Create(value, target));
        }

        [Fact]
        public void IntegerExampleSpaceFiltered()
        {
            var exampleSpace = ES.ExampleSpace
                .Unfold(10, ES.ShrinkFunc.Towards(0), x => x)
                .Where(x => x % 2 == 0);

            Snapshot.Match(exampleSpace.Render(x => x.ToString()));
        }
    }
}
