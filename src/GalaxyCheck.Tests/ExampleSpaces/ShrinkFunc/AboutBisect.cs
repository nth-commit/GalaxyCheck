using FsCheck;
using FsCheck.Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using ES = GalaxyCheck.Internal.ExampleSpaces;

namespace Tests.ExampleSpaces.ShrinkFunc
{
    public class AboutBisect
    {
        [Property]
        public void IfListIsEmpty_ItCannotShrink(NonNegativeInt minLength)
        {
            var shrink = ES.ShrinkFunc.Bisect<object>(minLength.Get);

            var result = shrink(new List<object>());

            ShrinkFuncAssert.CannotShrink(shrink, new List<object>());
        }

        [Property]
        public void IfListHasOneElement_ItCannotShrink(NonNegativeInt minLength, object value)
        {
            var shrink = ES.ShrinkFunc.Bisect<object>(minLength.Get);

            ShrinkFuncAssert.CannotShrink(shrink, new List<object> { value });
        }

        [Property]
        public void IfMinLengthIsEqualToListLength_ItCannotShrink(List<object> value)
        {
            var shrink = ES.ShrinkFunc.Bisect<object>(value.Count);

            ShrinkFuncAssert.CannotShrink(shrink, value);
        }

        [Property]
        public FsCheck.Property IfMinLengthIsGreaterThanHalfListLength_ItCannotShrink(NonNegativeInt minLength, List<object> value)
        {
            Action test = () =>
            {
                var shrink = ES.ShrinkFunc.Bisect<object>(minLength.Get);

                ShrinkFuncAssert.CannotShrink(shrink, value);
            };

            return test.When(minLength.Get > value.Count / 2);
        }

        public class Otherwise
        {
            private static bool ItCanShrink(NonNegativeInt minLength, List<object> value) =>
                value.Count >= 2 && value.Count / 2 >= minLength.Get;

            [Property]
            public FsCheck.Property ItNeverShrinksBelowMinLength(NonNegativeInt minLength, List<object> value)
            {
                Action test = () =>
                {
                    var shrink = ES.ShrinkFunc.Bisect<object>(minLength.Get);

                    ShrinkFuncAssert.ForAllShrinks(shrink, value, shrunkValue =>
                    {
                        Assert.True(shrunkValue.Count >= minLength.Get);
                    });
                };

                return test.When(ItCanShrink(minLength, value));
            }

            [Property]
            public FsCheck.Property ItHasTwoShrinks(NonNegativeInt minLength, List<object> value)
            {
                Action test = () =>
                {
                    var shrink = ES.ShrinkFunc.Bisect<object>(minLength.Get);

                    ShrinkFuncAssert.HasShrinkCount(shrink, value, 2);
                };

                return test.When(ItCanShrink(minLength, value));
            }

            [Property]
            public FsCheck.Property TheTwoShrinksHaveEvenlyDistributedLengths(NonNegativeInt minLength, List<object> value)
            {
                Action test = () =>
                {
                    var shrink = ES.ShrinkFunc.Bisect<object>(minLength.Get);

                    var result = shrink(value);

                    var length1 = result.First().Count;
                    var length2 = result.Skip(1).Single().Count;
                    Assert.True(Math.Abs(length1 - length2) <= 1);
                };

                return test.When(ItCanShrink(minLength, value));
            }

            [Property]
            public FsCheck.Property TheTwoShrinksCanRecreateTheOriginalList(NonNegativeInt minLength, List<object> value)
            {
                Action test = () =>
                {
                    var shrink = ES.ShrinkFunc.Bisect<object>(minLength.Get);

                    var result = shrink(value);

                    Assert.Equal(value, result.SelectMany(x => x));
                };

                return test.When(ItCanShrink(minLength, value));
            }
        }

        // TODO: Leverage this util in other ShrinkFunc tests
        public static class ShrinkFuncAssert
        {
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
        }
    }
}
