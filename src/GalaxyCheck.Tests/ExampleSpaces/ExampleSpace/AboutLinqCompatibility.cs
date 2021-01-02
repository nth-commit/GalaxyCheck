using Xunit;
using ES = GalaxyCheck.ExampleSpaces;

namespace GalaxyCheck.Tests.ExampleSpaces.ExampleSpace
{
    public class AboutLinqCompatibility
    {
        [Fact]
        public void ItSupportsFromKeyword()
        {
            var _ =
                from x in ES.ExampleSpace.Singleton(0)
                select x;
        }

        [Fact]
        public void ItSupportsWhereKeyword()
        {
            var _ =
                from x in ES.ExampleSpace.Singleton(0)
                where x % 2 == 0
                select x;
        }
    }
}
