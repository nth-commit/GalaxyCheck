using FsCheck;
using FsCheck.Xunit;
using GalaxyCheck.ExampleSpaces;
using System;
using Xunit;
using ES = GalaxyCheck.ExampleSpaces;

namespace GalaxyCheck.Tests.ExampleSpaces.ExampleSpace
{
    public class AboutAny
    {
        // Ideally the tests in this class would inject all the functions and also inject an `object` like the other
        // tests do, but FsCheck doesn't allow a conditional property if all the generated values are not comparable.
        // i.e. if you're injecting an object (or a function, for that matter), it can't figure out which parameter
        // you're trying to filter upon, and then gives up straight away.

        [Property]
        public Property WhenRootValuePassesThePredicate_ItReturnsTrue(int value)
        {
            Func<int, bool> pred = (x) => x % 2 == 0;

            Action test = () =>
            {
                var sourceExampleSpace = ES.ExampleSpace.Singleton(value).Where(pred);

                Assert.True(sourceExampleSpace.Any());
            };

            return test.When(pred(value));
        }

        [Property]
        public Property WhenRootValueFailsThePredicate_ItReturnsFalse(int value)
        {
            Func<int, bool> pred = (x) => x % 2 == 0;

            Action test = () =>
            {
                var sourceExampleSpace = ES.ExampleSpace.Singleton(value).Where(pred);

                Assert.False(sourceExampleSpace.Any());
            };

            return test.When(pred(value) == false);
        }
    }
}
