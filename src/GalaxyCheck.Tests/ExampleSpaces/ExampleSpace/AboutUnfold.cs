﻿using FsCheck.Xunit;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using GC = GalaxyCheck;

namespace Tests.ExampleSpaces.ExampleSpace
{
    public class AboutUnfold
    {
        [Property]
        public void ItRemovesDuplicates(int value)
        {
            static IEnumerable<int> shrink(int x) => new[] { x };

            var exampleSpace = GC.Internal.ExampleSpaces.ExampleSpace.Unfold(
                value,
                shrink,
                GC.Internal.ExampleSpaces.MeasureFunc.Unmeasured<int>());

            Assert.Equal(new [] { value }, exampleSpace.Sample().Select(problem => problem.Value));
        }

        [Property]
        public void ExampleOfUnfoldWithSimpleDecrementShrinker()
        {
            static IEnumerable<int> shrink(int x) => x <= 1 ? Enumerable.Empty<int>() : new[] { x - 1 };

            var exampleSpace = GC.Internal.ExampleSpaces.ExampleSpace.Unfold(
                10,
                shrink,
                GC.Internal.ExampleSpaces.MeasureFunc.Unmeasured<int>());

            Assert.Equal(
                new[] { 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 },
                exampleSpace.Sample().Select(problem => problem.Value));
        }
    }
}
