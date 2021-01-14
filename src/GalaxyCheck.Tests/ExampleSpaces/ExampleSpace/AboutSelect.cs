using FsCheck.Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using GC = GalaxyCheck;
using GalaxyCheck.Internal.ExampleSpaces;

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
            var sourceExampleSpace = GC.Internal.ExampleSpaces.ExampleSpace.Unfold(
                value,
                shrink.Invoke,
                measure.Invoke,
                IdentifyFuncs.Default<object>());

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
            var sourceExampleSpace = GC.Internal.ExampleSpaces.ExampleSpace.Unfold(
                value,
                shrink.Invoke,
                measure.Invoke,
                IdentifyFuncs.Default<object>());

            Assert.Equal(
                sourceExampleSpace.Sample().Select(example => example.Distance),
                sourceExampleSpace.Map(selector).Sample().Select(example => example.Distance));
        }

        [Property]
        public void ItDoesNotAffectId(
            object value,
            Func<object, List<object>> shrink,
            Func<object, decimal> measure,
            Func<object, object> selector)
        {
            var sourceExampleSpace = GC.Internal.ExampleSpaces.ExampleSpace.Unfold(
                value,
                shrink.Invoke,
                measure.Invoke,
                IdentifyFuncs.Default<object>());

            Assert.Equal(
                sourceExampleSpace.Sample().Select(example => example.Id),
                sourceExampleSpace.Map(selector).Sample().Select(example => example.Id));
        }
    }
}
