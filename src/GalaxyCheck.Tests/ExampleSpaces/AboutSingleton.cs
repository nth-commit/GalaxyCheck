using FsCheck.Xunit;
using GalaxyCheck.ExampleSpaces;
using Xunit;

namespace GalaxyCheck.Tests.ExampleSpaces
{
    public class AboutSingleton
    {
        [Property]
        public void ItContainsOneExample(object value)
        {
            var exampleSpace = ExampleSpace.Singleton(value);

            var example = Assert.Single(exampleSpace.TraverseGreedy());
            Assert.Equal(value, example.Value);
            Assert.Equal(0, example.Distance);
        }

        [Property]
        public void ItBehavesLikeUnfoldWithPrimitiveFunctions(object value)
        {
            var singletonExampleSpace = ExampleSpace.Singleton(value);
            var unfoldedExampleSpace = ExampleSpace.Unfold(value, ShrinkFunc.None<object>(), MeasureFunc.Unmeasured<object>());

            Assert.Equal(unfoldedExampleSpace.TraverseGreedy(), singletonExampleSpace.TraverseGreedy());
        }
    }
}
