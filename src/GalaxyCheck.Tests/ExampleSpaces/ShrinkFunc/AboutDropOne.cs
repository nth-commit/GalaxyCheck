using FsCheck;
using FsCheck.Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using ES = GalaxyCheck.Internal.ExampleSpaces;

namespace Tests.ExampleSpaces.ShrinkFunc
{
    public class AboutDropOne
    {
        [Property]
        public void IfListIsEmpty_ItCannotShrink(NonNegativeInt minLength)
        {
            var shrink = ES.ShrinkFunc.DropOne<object>(minLength.Get); 

            ShrinkFuncAssert.CannotShrink(shrink, new List<object> { });
        }

        [Property]
        public FsCheck.Property IfMinLengthIsGreaterThanOrEqualListLength_ItCannotShrink(NonNegativeInt minLength, List<object> value)
        {
            Action test = () =>
            {
                var shrink = ES.ShrinkFunc.DropOne<object>(minLength.Get);

                ShrinkFuncAssert.CannotShrink(shrink, value);
            };

            return test.When(minLength.Get >= value.Count);
        }

        public class Otherwise
        {
            [Property]
            public FsCheck.Property ItHasShrinksAsManyTimesAsTheListLength(NonNegativeInt minLength, List<object> value)
            {
                Action test = () =>
                {
                    var shrink = ES.ShrinkFunc.DropOne<object>(minLength.Get);

                    ShrinkFuncAssert.HasShrinkCount(shrink, value, value.Count);
                };

                return test.When(minLength.Get < value.Count);
            }

            [Property]
            public FsCheck.Property TheShrinksHaveOneLessLengthThanTheList(NonNegativeInt minLength, List<object> value)
            {
                Action test = () =>
                {
                    var shrink = ES.ShrinkFunc.DropOne<object>(minLength.Get);

                    ShrinkFuncAssert.ForAllShrinks(shrink, value, x =>
                    {
                        Assert.Equal(value.Count - 1, x.Count);
                    });
                };

                return test.When(minLength.Get < value.Count);
            }

            [Property]
            public FsCheck.Property TheShrinksAreDistinct(NonNegativeInt minLength, List<int> value)
            {
                value = value.Select((x, i) => i).ToList();

                Action test = () =>
                {
                    var shrink = ES.ShrinkFunc.DropOne<int>(minLength.Get);

                    ShrinkFuncAssert.DoesNotShrinksToDuplicates(shrink, value);
                };

                return test.When(minLength.Get < value.Count);
            }
        }
    }
}
