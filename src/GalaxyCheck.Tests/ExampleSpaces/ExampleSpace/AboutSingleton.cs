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
            var exampleSpace = GC.Internal.ExampleSpaces.ExampleSpace.Singleton(value);

            var example = Assert.Single(exampleSpace.Sample());
            Assert.Equal(value, example.Value);
            Assert.Equal(0, example.Distance);
        }

        [Property]
        public void ItBehavesLikeUnfoldWithPrimitiveFunctions(object value)
        {
            var singletonExampleSpace = GC.Internal.ExampleSpaces.ExampleSpace.Singleton(value);
            var unfoldedExampleSpace = GC.Internal.ExampleSpaces.ExampleSpace.Unfold(
                value,
                GC.Internal.ExampleSpaces.ShrinkFunc.None<object>(),
                GC.Internal.ExampleSpaces.MeasureFunc.Unmeasured<object>(),
                GC.Internal.ExampleSpaces.IdentifyFuncs.Constant<object>());

            Assert.Equal(unfoldedExampleSpace.Sample(), singletonExampleSpace.Sample());
        }
    }
}
