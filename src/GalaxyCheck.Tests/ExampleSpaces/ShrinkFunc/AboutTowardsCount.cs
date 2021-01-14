using System.Linq;
using Xunit;
using ES = GalaxyCheck.Internal.ExampleSpaces;

namespace Tests.ExampleSpaces.ShrinkFunc
{
    public class AboutTowardsCount
    {
        [Theory]
        [InlineData("a", 0, "")]
        [InlineData("ab", 0, ",a,b")]
        [InlineData("ab", 1, "a,b")]
        [InlineData("abc", 0, ",ab,ac,bc")]
        [InlineData("abc", 1, "a,ab,b,c,ac,bc")]
        [InlineData("abc", 2, "ab,ac,bc")]
        public void Examples(string source, int k, string expectedCombinationsDelimited)
        {
            var shrink = ES.ShrinkFunc.TowardsCount<string>(k);

            var result = shrink(source.AsEnumerable().Select(x => x.ToString())).Select(c => string.Join("", c));

            var expectedCombinations = expectedCombinationsDelimited.Split(',');
            Assert.Equal(expectedCombinations, result);
        }
    }
}
