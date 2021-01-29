using System.Linq;
using Xunit;
using GalaxyCheck.Internal.ExampleSpaces;
using ES = GalaxyCheck.Internal.ExampleSpaces;

namespace Tests.ExampleSpaces.ShrinkFunc
{
    public class AboutTowardsCount
    {
        [Theory]
        [InlineData("a", 0, "")]
        [InlineData("ab", 0, ",a,b")]
        [InlineData("ba", 0, "ab,,b,a")]
        [InlineData("ab", 1, "a,b")]
        [InlineData("ba", 1, "ab,b,a")]
        [InlineData("abc", 0, ",ab,ac,bc")]
        [InlineData("abc", 1, "a,ab,b,c,ac,bc")]
        [InlineData("abc", 2, "ab,ac,bc")]
        public void Examples(string source, int k, string expectedCombinationsDelimited)
        {
            var shrink = ES.ShrinkFunc
                .TowardsCount<string, string>(k, x => x)
                .Map(x => string.Join("", x), x => x.Select(c => c.ToString()).ToList());

            ShrinkFuncAssert.ShrinksTo(shrink, source, expectedCombinationsDelimited.Split(','));
        }
    }
}
