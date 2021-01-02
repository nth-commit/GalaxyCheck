using FsCheck;
using FsCheck.Xunit;
using System;
using System.Linq;
using Xunit;
using ES = GalaxyCheck.ExampleSpaces;

namespace GalaxyCheck.Tests.ExampleSpaces.ShrinkFunc
{
    public class AboutShrinkTowards
    {
        [Property]
        public Property ItReturnsAnEnumerableWhereTheFirstIsTheTarget(int value, int target)
        {
            Action test = () =>
            {
                var shrink = ES.ShrinkFunc.Towards(target);

                var first = shrink(value).First();

                Assert.Equal(target, first);
            };

            return test.When(value != target);
        }

        [Property]
        public void ItReturnsAnEnumerableWhichDoesNotContainTheValue(int value, int target)
        {
            var shrink = ES.ShrinkFunc.Towards(target);

            var result = shrink(value);

            Assert.DoesNotContain(value, result);
        }

        [Property]
        public void ItReturnsAnEnumerableWhichDoesNotContainDuplicates(int value, int target)
        {
            var shrink = ES.ShrinkFunc.Towards(target);

            var result = shrink(value);

            Assert.Equal(result.Distinct(), result);
        }

        [Property]
        public void WhenTheValueIsTheTarget_ItReturnsAnEmptyEnumerable(int target)
        {
            var shrink = ES.ShrinkFunc.Towards(target);

            var result = shrink(target);

            Assert.Empty(result);
        }

        [Theory]
        [InlineData(0, 0, new int[] { })]
        [InlineData(10, 0, new int[] { 0, 5, 8, 9 })]
        [InlineData(0, 10, new int[] { 10, 5, 2, 1 })]
        [InlineData(100, 0, new int[] { 0, 50, 75, 88, 94, 97, 99 })]
        public void Examples(int value, int target, int[] expected)
        {
            var shrink = ES.ShrinkFunc.Towards(target);

            var actual = shrink(value);

            Assert.Equal(expected.ToList(), actual.ToList());
        }
    }
}
