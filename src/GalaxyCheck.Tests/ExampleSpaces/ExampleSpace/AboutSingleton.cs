using FsCheck.Xunit;
using Xunit;
using GC = GalaxyCheck;

namespace Tests.ExampleSpaces.ExampleSpace
{
    public class AboutSingleton
    {
        [Property]
        public void ItContainsOneExample(object value)
        {
            var exampleSpace = GC.ExampleSpaces.ExampleSpace.Singleton(value);

            var example = Assert.Single(exampleSpace.Sample());
            Assert.Equal(value, example.Value);
            Assert.Equal(0, example.Distance);
        }

        [Property]
        public void ItBehavesLikeUnfoldWithPrimitiveFunctions(object value)
        {
            var singletonExampleSpace = GC.ExampleSpaces.ExampleSpace.Singleton(value);
            var unfoldedExampleSpace = GC.ExampleSpaces.ExampleSpace.Unfold(
                value,
                GC.ExampleSpaces.ShrinkFunc.None<object>(),
                GC.ExampleSpaces.MeasureFunc.Unmeasured<object>());

            Assert.Equal(unfoldedExampleSpace.Sample(), singletonExampleSpace.Sample());
        }
    }
}
