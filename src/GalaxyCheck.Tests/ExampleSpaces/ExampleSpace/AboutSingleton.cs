using FsCheck.Xunit;
using GalaxyCheck.ExampleSpaces;
using Xunit;

namespace Tests.ExampleSpaces.ExampleSpace
{
    public class AboutSingleton
    {
        [Property]
        public void ItContainsOneExample(object value)
        {
            var exampleSpace = ExampleSpaceFactory.Singleton(value);

            var example = Assert.Single(exampleSpace.Sample());
            Assert.Equal(value, example.Value);
            Assert.Equal(0, example.Distance);
        }

        [Property]
        public void ItBehavesLikeUnfoldWithPrimitiveFunctions(object value)
        {
            var singletonExampleSpace = ExampleSpaceFactory.Singleton(value);
            var unfoldedExampleSpace = ExampleSpaceFactory.Unfold(
                value,
                ShrinkFunc.None<object>(),
                MeasureFunc.Unmeasured<object>(),
                IdentifyFuncs.Constant<object>());

            Assert.Equal(unfoldedExampleSpace.Sample(), singletonExampleSpace.Sample());
        }
    }
}
