using FsCheck.Xunit;
using GalaxyCheck.ExampleSpaces;
using System.Linq;
using Xunit;
using ES = GalaxyCheck.ExampleSpaces;

namespace GalaxyCheck.Tests.ExampleSpaces.ExampleSpace
{
    public class AboutUnfold
    {
        [Property]
        public void ExampleOfUnfoldWithSimpleDecrementShrinker()
        {
            ShrinkFunc<int> shrink = (x) => x <= 1 ? Enumerable.Empty<int>() : new[] { x - 1 };
            var exampleSpace = ES.ExampleSpace.Unfold(10, shrink, MeasureFunc.Unmeasured<int>());

            Assert.Equal(
                new[] { 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 },
                exampleSpace.TraverseGreedy().Select(problem => problem.Value));
        }
    }
}
