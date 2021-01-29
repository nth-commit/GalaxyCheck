using FsCheck;
using FsCheck.Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using ES = GalaxyCheck.Internal.ExampleSpaces;

namespace Tests.ExampleSpaces.ShrinkFunc
{
    public class AboutOrder
    {
        [Property]
        public FsCheck.Property IfInputEnumerableIsSorted_ItCannotShrink(
            List<int> xs)
        {
            Action test = () =>
            {
                var shrink = ES.ShrinkFunc.Order<int, int>(x => x);

                ShrinkFuncAssert.CannotShrink(shrink, xs.OrderBy(x => x).ToList());
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

            ShrinkFuncAssert.ShrinksToOne(shrink, unsorted.ToList(), sorted.ToList());
        }
    }
}
