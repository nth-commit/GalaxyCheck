using FluentAssertions;
using GalaxyCheck.ExampleSpaces;
using NebulaCheck;
using NebulaCheck.Xunit;
using System.Linq;
using Xunit;

namespace Tests.V2.ShrinkTests
{
    public class AboutOtherCombinations
    {
        [Theory]
        [InlineData("abc", 2, "ab,ac,bc")]
        [InlineData("abcd", 2, "ab,ac,ad,bc,bd,cd")]
        [InlineData("abcd", 3, "abc,abd,acd,bcd")]
        [InlineData("abcde", 2, "ab,ac,ad,ae,bc,bd,be,cd,ce,de")]
        public void Examples(string value, int k, string expectedShrinksDelimited)
        {
            var func = ShrinkFunc.OtherCombinations<char>(k);

            var shrinks = func(value.ToList()).Select(chars => new string(chars.ToArray()));

            shrinks.Should().BeEquivalentTo(expectedShrinksDelimited.Split(","));
        }

        [Property]
        public IGen<Test> IfListCountIsLessThanEqualK_ItWillNotShrink() =>
            from k in Gen.Int32().Between(1, 20)
            from list in DomainGen.AnyList().WithCountLessThanEqual(k)
            select Property.ForThese(() =>
            {
                var func = ShrinkFunc.OtherCombinations<object>(k);

                var shrinks = func(list.ToList());

                shrinks.Should().BeEmpty();
            });

        [Property]
        public IGen<Test> IfListCountIsGreaterThanK_ItWillShrink() =>
            from k in Gen.Int32().Between(1, 20)
            from list in DomainGen.AnyList().WithCountGreaterThan(k)
            select Property.ForThese(() =>
            {
                var func = ShrinkFunc.OtherCombinations<object>(k);

                var shrinks = func(list.ToList());

                shrinks.Should().NotBeEmpty();
            });

        [Property]
        public IGen<Test> IfKIsOne_ItProducesAListOfEachSingleElement() =>
            from list in DomainGen.AnyList().WithCountGreaterThanEqual(2)
            select Property.ForThese(() =>
            {
                var func = ShrinkFunc.OtherCombinations<object>(1);

                var shrinks = func(list.ToList());

                shrinks.Should().HaveCount(list.Count);
                shrinks.SelectMany(x => x).Should().BeEquivalentTo(list);
            });
    }
}
