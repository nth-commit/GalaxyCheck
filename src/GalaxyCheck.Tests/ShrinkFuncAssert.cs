using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using ES = GalaxyCheck.ExampleSpaces;

namespace Tests
{
    public static class ShrinkFuncAssert
    {
        public static void CanShrink<T>(ES.ShrinkFunc<T> shrink, T value)
        {
            var result = shrink(value);
            Assert.NotEmpty(result);
        }

        public static void CannotShrink<T>(ES.ShrinkFunc<T> shrink, T value)
        {
            var result = shrink(value);
            Assert.Empty(result);
        }

        public static void HasShrinkCount<T>(ES.ShrinkFunc<T> shrink, T value, int expectedShrinkCount)
        {
            var result = shrink(value);
            Assert.Equal(expectedShrinkCount, result.Count());
        }

        public static void ForAllShrinks<T>(ES.ShrinkFunc<T> shrink, T value, Action<T> assert)
        {
            var result = shrink(value);
            Assert.All(result, assert);
        }

        public static void ShrinksTo<T>(ES.ShrinkFunc<T> shrink, T value, IEnumerable<T> expectedShrinks)
        {
            var result = shrink(value);
            Assert.Equal(result, expectedShrinks);
        }

        public static void ShrinksToOne<T>(ES.ShrinkFunc<T> shrink, T value, T expectedShrink)
        {
            ShrinksTo(shrink, value, new[] { expectedShrink });
        }

        public static void ShrinksToFirst<T>(ES.ShrinkFunc<T> shrink, T value, T expectedShrink)
        {
            var result = shrink(value);
            Assert.Equal(result.First(), expectedShrink);
        }

        public static void DoesNotShrinkTo<T>(ES.ShrinkFunc<T> shrink, T value, T neverValue)
        {
            var result = shrink(value);
            Assert.DoesNotContain(neverValue, result);
        }

        public static void DoesNotShrinksToDuplicates<T>(ES.ShrinkFunc<T> shrink, T value)
        {
            // I-i-i know you'd like to think yo shrink dis-tiiiiinct, but lean a lil bit closer, see... 
            var result = shrink(value);
            Assert.Equal(
                result.Select(x => JsonConvert.SerializeObject(x)).Distinct(),
                result.Select(x => JsonConvert.SerializeObject(x)));
            // Rose trees never have du-du-plicates *cringe*
        }
    }
}
