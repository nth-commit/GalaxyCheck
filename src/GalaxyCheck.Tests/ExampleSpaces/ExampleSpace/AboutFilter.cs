using FsCheck;
using FsCheck.Xunit;
using System;
using Xunit;
using GC = GalaxyCheck;
using GalaxyCheck.ExampleSpaces;

namespace Tests.ExampleSpaces.ExampleSpace
{
    public class AboutFilter
    {
        // Ideally the tests in this class would inject all the functions and also inject an `object` like the other
        // tests do, but FsCheck doesn't allow a conditional property if all the generated values are not comparable.
        // i.e. if you're injecting an object (or a function, for that matter), it can't figure out which parameter
        // you're trying to filter upon, and then gives up straight away.

        [Property]
        public FsCheck.Property WhenRootValuePassesThePredicate_ItReturnsNotNull(int value)
        {
            static bool pred(int x) => x % 2 == 0;

            Action test = () =>
            {
                var sourceExampleSpace = GC.ExampleSpaces.ExampleSpace.Singleton(value).Filter(pred);

                Assert.NotNull(sourceExampleSpace);
            };

            return test.When(pred(value));
        }

        [Property]
        public FsCheck.Property WhenRootValueFailsThePredicate_ItReturnsFalse(int value)
        {
            static bool pred(int x) => x % 2 == 0;

            Action test = () =>
            {
                var sourceExampleSpace = GC.ExampleSpaces.ExampleSpace.Singleton(value).Filter(pred);

                Assert.Null(sourceExampleSpace);
            };

            return test.When(pred(value) == false);
        }
    }
}
