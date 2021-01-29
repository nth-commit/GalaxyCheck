using FsCheck;
using FsCheck.Xunit;
using System;
using System.Linq;
using Xunit;
using GC = GalaxyCheck;

namespace Tests.ExampleSpaces.ShrinkFunc
{
    public class AboutTowards
    {
        [Property]
        public FsCheck.Property ItProducesTheTargetValueFirst(int value, int target)
        {
            Action test = () =>
            {
                var shrink = GC.Internal.ExampleSpaces.ShrinkFunc.Towards(target);

                ShrinkFuncAssert.ShrinksToFirst(shrink, value, target);
            };

            return test.When(value != target);
        }

        [Property]
        public void ItDoesNotProduceTheOriginalValue(int value, int target)
        {
            var shrink = GC.Internal.ExampleSpaces.ShrinkFunc.Towards(target);

            ShrinkFuncAssert.DoesNotShrinkTo(shrink, value, value);
        }

        [Property]
        public void ItDoesNotProduceDuplicates(int value, int target)
        {
            var shrink = GC.Internal.ExampleSpaces.ShrinkFunc.Towards(target);

            ShrinkFuncAssert.DoesNotShrinksToDuplicates(shrink, value);
        }

        [Property]
        public void ItReflectsAroundZero(int value)
        {
            var shrink = GC.Internal.ExampleSpaces.ShrinkFunc.Towards(0);

            var result0 = shrink(value).Select(x => Math.Abs(x));
            var result1 = shrink(-value).Select(x => Math.Abs(x));

            Assert.Equal(result0, result1);
        }

        [Property]
        public void WhenTheValueIsTheTarget_ItCannotShrink(int target)
        {
            var shrink = GC.Internal.ExampleSpaces.ShrinkFunc.Towards(target);

            ShrinkFuncAssert.CannotShrink(shrink, target);
        }

        [Theory]
        [InlineData(0, 0, new int[] { })]
        [InlineData(10, 0, new int[] { 0, 5, 8, 9 })]
        [InlineData(0, 10, new int[] { 10, 5, 2, 1 })]
        [InlineData(100, 0, new int[] { 0, 50, 75, 88, 94, 97, 99 })]
        public void Examples(int value, int target, int[] expected)
        {
            var shrink = GC.Internal.ExampleSpaces.ShrinkFunc.Towards(target);

            ShrinkFuncAssert.ShrinksTo(shrink, value, expected);
        }
    }
}
