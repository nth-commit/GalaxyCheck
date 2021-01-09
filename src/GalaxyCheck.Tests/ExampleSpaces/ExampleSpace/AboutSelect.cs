using FsCheck.Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using GC = GalaxyCheck;
using GalaxyCheck.ExampleSpaces;

namespace Tests.ExampleSpaces.ExampleSpace
{
    public class AboutSelect
    {
        [Property]
        public void ItBehavesLikeALinqSelectOnValues(
            object value,
            Func<object, List<object>> shrink,
            Func<object, decimal> measure,
            Func<object, object> selector)
        {
            var sourceExampleSpace = GC.ExampleSpaces.ExampleSpace.Unfold(value, shrink.Invoke, measure.Invoke);

            Assert.Equal(
                sourceExampleSpace.Sample().Select(example => selector(example.Value)),
                sourceExampleSpace.Map(selector).Sample().Select(example => example.Value));
        }

        [Property]
        public void ItDoesNotAffectDistance(
            object value,
            Func<object, List<object>> shrink,
            Func<object, decimal> measure,
            Func<object, object> selector)
        {
            var sourceExampleSpace = GC.ExampleSpaces.ExampleSpace.Unfold(value, shrink.Invoke, measure.Invoke);

            Assert.Equal(
                sourceExampleSpace.Sample().Select(example => selector(example.Value)),
                sourceExampleSpace.Map(selector).Sample().Select(example => example.Value));
        }
    }
}
