using FsCheck.Xunit;
using GalaxyCheck.ExampleSpaces;
using Xunit;
using ES = GalaxyCheck.ExampleSpaces;

namespace GalaxyCheck.Tests.ExampleSpaces.ExampleSpace
{
    public class AboutSingleton
    {
        [Property]
        public void ItContainsOneExample(object value)
        {
            var exampleSpace = ES.ExampleSpace.Singleton(value);

            var example = Assert.Single(exampleSpace.TraverseGreedy());
            Assert.Equal(value, example.Value);
            Assert.Equal(0, example.Distance);
        }

        [Property]
        public void ItBehavesLikeUnfoldWithPrimitiveFunctions(object value)
        {
            var singletonExampleSpace = ES.ExampleSpace.Singleton(value);
            var unfoldedExampleSpace = ES.ExampleSpace.Unfold(value, ES.ShrinkFunc.None<object>(), MeasureFunc.Unmeasured<object>());

            Assert.Equal(unfoldedExampleSpace.TraverseGreedy(), singletonExampleSpace.TraverseGreedy());
        }
    }
}
