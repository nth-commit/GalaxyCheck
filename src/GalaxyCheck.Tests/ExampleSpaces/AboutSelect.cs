using FsCheck.Xunit;
using GalaxyCheck.ExampleSpaces;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace GalaxyCheck.Tests.ExampleSpaces
{
    public class AboutSelect
    {
        [Property]
        public void ItBehavesLikeALinqSelectOnValues(
            object value,
            Func<object, List<object>> shrink,
            Func<object, int> measure,
            Func<object, object> selector)
        {
            var sourceExampleSpace = ExampleSpace.Unfold(value, shrink.Invoke, measure.Invoke);

            Assert.Equal(
                sourceExampleSpace.TraverseGreedy().Select(example => selector(example.Value)),
                sourceExampleSpace.Select(selector).TraverseGreedy().Select(example => example.Value));
        }

        [Property]
        public void ItDoesNotAffectDistance(
            object value,
            Func<object, List<object>> shrink,
            Func<object, int> measure,
            Func<object, object> selector)
        {
            var sourceExampleSpace = ExampleSpace.Unfold(value, shrink.Invoke, measure.Invoke);

            Assert.Equal(
                sourceExampleSpace.TraverseGreedy().Select(example => selector(example.Value)),
                sourceExampleSpace.Select(selector).TraverseGreedy().Select(example => example.Value));
        }
    }
}
