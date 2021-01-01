using FsCheck.Xunit;
using GalaxyCheck.ExampleSpaces;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace GalaxyCheck.Tests.ExampleSpaces
{
    public class AboutWhere
    {
        [Property]
        public void ItBehavesLikeALinqWhere(
            object value,
            Func<object, List<object>> shrink,
            Func<object, int> measure,
            Func<object, bool> pred)
        {
            var sourceExampleSpace = ExampleSpace.Unfold(value, shrink.Invoke, measure.Invoke);

            // Limitation of either the FsCheck function generator, or two generated functions interacting with each
            // other, is that it can converge on a value that never passes the predicate, which causes a crash (will
            // generate forever). Get around this by restricting the value set before the LINQ filter, then pulling
            // the same number of values for the ExampleSpace filter.
            var linqWhereExamples = sourceExampleSpace.TraverseGreedy().Where(example => pred(example.Value)).ToList();
            var exampleSpaceWhereExamples = sourceExampleSpace.Where(pred).TraverseGreedy(linqWhereExamples.Count);

            Assert.Equal(linqWhereExamples, exampleSpaceWhereExamples);
        }
    }
}
