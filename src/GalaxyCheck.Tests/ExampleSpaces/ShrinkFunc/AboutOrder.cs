using FsCheck;
using FsCheck.Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using ES = GalaxyCheck.Internal.ExampleSpaces;

namespace Tests.ExampleSpaces.ShrinkFunc
{
    public class AboutOrder
    {
        [Property]
        public FsCheck.Property IfInputEnumerableIsSorted_ItReturnsEmptyEnumerable(
            List<int> xs)
        {
            Action test = () =>
            {
                var xs0 = xs.OrderBy(x => x);
                var shrink = ES.ShrinkFunc.Order<int, int>(x => x);

                var result = shrink(xs0);

                Assert.Empty(result);
            };

            return test.When(xs.Count > 1);
        }

        [Theory]
        [InlineData(new [] { 1, 0 }, new [] { 0, 1 })]
        [InlineData(new [] { 1, 0, 1 }, new [] { 0, 1, 1 })]
        public void IfInputEnumerableIsNotSorted_ItReturnsAOneElementEnumerableContainingTheSortedInput(
            int[] unsorted,
            int[] sorted)
        {
            var shrink = ES.ShrinkFunc.Order<int, int>(x => x);

            var result = shrink(unsorted);

            Assert.Equal(result, new[] { sorted });
        }
    }
}
