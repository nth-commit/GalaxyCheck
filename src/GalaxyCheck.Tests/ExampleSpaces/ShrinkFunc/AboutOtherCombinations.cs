using FsCheck;
using FsCheck.Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using ES = GalaxyCheck.Internal.ExampleSpaces;

namespace Tests.ExampleSpaces.ShrinkFunc
{
    public class AboutOtherCombinations
    {
        [Property]
        public FsCheck.Property IfCombinationSizeIsGreaterThanOrEqualEnumerableSize_ItReturnsEmptyEnumerable(List<object> source, int k)
        {
            Action test = () =>
            {
                var shrink = ES.ShrinkFunc.OtherCombinations<object>(k);

                ShrinkFuncAssert.CannotShrink(shrink, source);
            };

            return test.When(k >= source.Count);
        }

        [Property]
        public FsCheck.Property IfCombinationSizeIsLessThanEnumerableSize_ItReturnsNonEmptyEnumerable(List<object> source, int k)
        {
            Action test = () =>
            {
                var shrink = ES.ShrinkFunc.OtherCombinations<object>(k);

                ShrinkFuncAssert.CanShrink(shrink, source);
            };

            return test.When(k > 0 && k < source.Count);
        }

        [Property]
        public FsCheck.Property IfCombinationSizeIsOne_ItReturnsAnEnumerableOfEachElement(List<object> source)
        {
            Action test = () =>
            {
                var shrink = ES.ShrinkFunc.OtherCombinations<object>(1);

                var result = shrink(source).SelectMany(x => x);

                Assert.Equal(result, source);
            };

            return test.When(source.Count > 1);
        }

        [Theory]
        [InlineData("abc", 2, "ab,ac,bc")]
        [InlineData("abcd", 2, "ab,ac,ad,bc,bd,cd")]
        [InlineData("abcd", 3, "abc,abd,acd,bcd")]
        [InlineData("abcde", 2, "ab,ac,ad,ae,bc,bd,be,cd,ce,de")]
        public void Examples(string source, int k, string expectedCombinationsDelimited)
        {
            var shrink = ES.ShrinkFunc.OtherCombinations<string>(k);

            var result = shrink(source.Select(x => x.ToString()).ToList()).Select(c => string.Join("", c));

            var expectedCombinations = expectedCombinationsDelimited.Split(',');
            Assert.Equal(expectedCombinations, result);
        }
    }
}
