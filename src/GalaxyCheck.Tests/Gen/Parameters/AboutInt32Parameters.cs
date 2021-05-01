using System;
using System.Reflection;
using Xunit;
using GalaxyCheck;
using GalaxyCheck.Gens.Injection.Int32;
using GC = GalaxyCheck;
using static Tests.TestUtils;

namespace Tests.Gen.Parameters
{
    public class AboutInt32Parameters
    {
        [Fact]
        public void ItGeneratesParametersLikeTheDefaultInt32Generator()
        {
            TestWithSeed(seed =>
            {
                Action<int> f = x => { };

                var gen = GC.Gen.Parameters(f.Method);

                var expectedGen = GC.Gen.Int32();
                GenAssert.Equal(expectedGen.Select(x => new object[] { x }), gen, seed);
            });
        }

        public class AboutGreaterThanEqual
        {
            private static void GreaterThanEqual0Example([GreaterThanEqual(0)] int _) { }
            private static void GreaterThanEqual10Example([GreaterThanEqual(10)] int _) { }

            [Theory]
            [InlineData(nameof(GreaterThanEqual0Example), 0)]
            [InlineData(nameof(GreaterThanEqual10Example), 10)]
            public void ItIsSupportedViaAnAttribute(string methodName, int expectedMin)
            {
                TestWithSeed(seed =>
                {
                    var methodInfo = typeof(AboutGreaterThanEqual).GetMethod(
                        methodName,
                        BindingFlags.Static | BindingFlags.NonPublic)!;

                    var gen = GC.Gen.Parameters(methodInfo);

                    var expectedGen = GC.Gen.Int32().GreaterThanEqual(expectedMin);
                    GenAssert.Equal(expectedGen.Select(x => new object[] { x }), gen, seed);
                });
            }
        }

        public class AboutLessThanEqual
        {
            private static void LessThanEqual0Example([LessThanEqual(0)] int _) { }
            private static void LessThanEqual10Example([LessThanEqual(10)] int _) { }

            [Theory]
            [InlineData(nameof(LessThanEqual0Example), 0)]
            [InlineData(nameof(LessThanEqual10Example), 10)]
            public void ItIsSupportedViaAnAttribute(string methodName, int expectedMax)
            {
                TestWithSeed(seed =>
                {
                    var methodInfo = typeof(AboutLessThanEqual).GetMethod(
                        methodName,
                        BindingFlags.Static | BindingFlags.NonPublic)!;

                    var gen = GC.Gen.Parameters(methodInfo);

                    var expectedGen = GC.Gen.Int32().LessThanEqual(expectedMax);
                    GenAssert.Equal(expectedGen.Select(x => new object[] { x }), gen, seed);
                });
            }
        }

        public class AboutBetween
        {
            private static void Between0And5Example([Between(0, 5)] int _) { }
            private static void Between5And10Example([Between(5, 10)] int _) { }

            [Theory]
            [InlineData(nameof(Between0And5Example), 0, 5)]
            [InlineData(nameof(Between5And10Example), 5, 10)]
            public void ItIsSupportedViaAnAttribute(string methodName, int expectedX, int expectedY)
            {
                TestWithSeed(seed =>
                {
                    var methodInfo = typeof(AboutBetween).GetMethod(
                        methodName,
                        BindingFlags.Static | BindingFlags.NonPublic)!;

                    var gen = GC.Gen.Parameters(methodInfo);

                    var expectedGen = GC.Gen.Int32().Between(expectedX, expectedY);
                    GenAssert.Equal(expectedGen.Select(x => new object[] { x }), gen, seed);
                });
            }
        }
    }
}
