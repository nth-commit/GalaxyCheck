using Xunit;
using GC = GalaxyCheck;

namespace Tests.ExampleSpaces.ExampleSpace
{
    public class AboutLinqCompatibility
    {
        [Fact]
        public void ItSupportsFromKeyword()
        {
            var _ =
                from x in GC.ExampleSpaces.ExampleSpace.Singleton(0)
                select x;
        }

        [Fact]
        public void ItSupportsWhereKeyword()
        {
            var _ =
                from x in GC.ExampleSpaces.ExampleSpace.Singleton(0)
                where x % 2 == 0
                select x;
        }
    }
}
