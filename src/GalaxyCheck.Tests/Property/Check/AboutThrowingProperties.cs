using FsCheck.Xunit;
using GalaxyCheck;
using System;
using Xunit;
using static Tests.TestUtils;

namespace Tests.Property.Check
{
    [Properties(Arbitrary = new[] { typeof(ArbitraryGen) })]
    public class AboutThrowingProperties
    {
        [Property]
        public void ItTranslatesNonThrowingPropertyActionsToAnUnfalsifiedResult(IGen<object> gen)
        {
            Action<object> nonThrowingPropertyAction = (_) =>
            {
            };

            TestWithSeed(seed =>
            {
                var property = gen.ForAll(nonThrowingPropertyAction);

                PropertyAssert.DoesNotFalsify(property, seed);
            });
        }

        [Property]
        public void ItTranslatesThrowingPropertyActionsToAnFalsifiedResult(IGen<object> gen, FsCheck.NonNull<string> message)
        {
            Action<object> throwingPropertyAction = (_) =>
            {
                throw new Exception(message.Get);
            };

            TestWithSeed(seed =>
            {
                var property = gen.ForAll(throwingPropertyAction);

                var (_, falsified) = PropertyAssert.Falsifies(property, seed);
                Assert.Equal(message.Get, falsified?.Exception?.Message);
            });
        }

        [Property]
        public void ItTranslatesThrowingPropertyFunctionsToAnFalsifiedResult(IGen<object> gen, FsCheck.NonNull<string> message)
        {
            Func<object, bool> throwingPropertyFunc = (_) =>
            {
                throw new Exception(message.Get);
            };

            TestWithSeed(seed =>
            {
                var property = gen.ForAll(throwingPropertyFunc);

                var (_, falsified) = PropertyAssert.Falsifies(property, seed);
                Assert.Equal(message.Get, falsified?.Exception?.Message);
            });
        }
    }
}
