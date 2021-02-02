using FsCheck;
using FsCheck.Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using GalaxyCheck;
using GC = GalaxyCheck;
using static Tests.TestUtils;

namespace Tests.Gen.Function
{
    /// <summary>
    /// Test trivia:
    /// 
    /// The number one requirement of generated functions is that they are (or at least appear) pure. This test module
    /// checks that by induction - first testing the nullary function, and leveraging those properties to make an
    /// assertion about the unary function, and so on, and so forth.
    /// 
    /// It's easy to test if the nullary function is pure - it takes no arguments. If you call it twice, you should see
    /// the same result.
    /// 
    /// Now, because we know the nullary function is pure, we can use that to test if the unary function is pure. We can
    /// do this by controlling its argument, calling it, and checking if its result is equal to the nullary function 
    /// with the same seed.
    /// 
    /// But how do we know that the unary function produces values with adequate variance? It could just be faking it
    /// and by the nullary argument in disguise (ignoring the argument and always returning the same value). That's easy,
    /// we just need to call it with a wide range of values, and make sure that it doesn't always return the same thing.
    /// 
    /// We can continue this process to test each function, by controlling more and more arguments as the arity
    /// increases.
    /// </summary>
    [Properties(MaxTest = 10)]
    public class AboutValueProduction
    {
        /// <summary>
        /// The controlling argument is based on the slight implementation detail that the hash of 0 is always 0, and
        /// when combining hashes, the hash of 0 is a no-op. I would consider this only "slightly" an implementation
        /// detail of GalaxyCheck, because it's more about the wider knowledge of hashes behave in .NET in general.
        /// It is an implementation detail that GalaxyCheck uses a hash to ensure purity of generated functions, but
        /// it's probably the only viable solution, so it's unlikely to change.
        /// </summary>
        private const int ControlArg = 0;

        public class NullaryFunction
        {
            [Fact]
            public void ItIsPure()
            {
                var returnGen = GC.Gen.Int32().Between(0, 10).WithBias(GC.Gen.Bias.None);
                var gen = GC.Gen.Function(returnGen);

                var func = gen.SampleOne(seed: 0);

                Assert.Equal(func(), func());
            }
        }

        public class UnaryFunction
        {
            [Fact]
            public void WhenTheArgumentIsControlled_ItBehavesLikeTheNullaryFunction()
            {
                TestWithSeed(seed =>
                {
                    var returnGen = GC.Gen.Int32().Between(0, 10).WithBias(GC.Gen.Bias.None);

                    var gen0 = GC.Gen.Function(returnGen);
                    var gen1 = GC.Gen.Function<object, int>(returnGen);

                    var func0 = gen0.SampleOne(seed: seed);
                    var func1 = gen1.SampleOne(seed: seed);

                    Assert.Equal(func0(), func1(ControlArg));
                });
            }

            [Property]
            public FsCheck.Property ItIsEffectedByItsArgument(List<object> variable0)
            {
                Action test = () => TestWithSeed(seed =>
                {
                    var returnGen = GC.Gen.Int32().Between(0, 10).WithBias(GC.Gen.Bias.None);
                    var gen = GC.Gen.Function<object, int>(returnGen);

                    var func = gen.SampleOne(seed: 0);

                    Assert.True(variable0.Select(func).Distinct().Count() > 1);
                });

                return test.When(variable0.Distinct().Count() >= 10);
            }
        }

        public class BinaryFunction
        {
            [Property]
            public void WhenTheSecondArgumentIsControlled_ItBehavesLikeTheUnaryFunction(object arg0)
            {
                TestWithSeed(seed =>
                {
                    var returnGen = GC.Gen.Int32().Between(0, 10).WithBias(GC.Gen.Bias.None);

                    var gen0 = GC.Gen.Function<object, int>(returnGen);
                    var gen1 = GC.Gen.Function<object, object, int>(returnGen);

                    var func0 = gen0.SampleOne(seed: seed);
                    var func1 = gen1.SampleOne(seed: seed);

                    Assert.Equal(func0(arg0), func1(arg0, ControlArg));
                });
            }

            [Property]
            public FsCheck.Property ItIsEffectedByItsSecondArgument(object arg0, List<object> variable1)
            {
                Action test = () => TestWithSeed(seed =>
                {
                    var returnGen = GC.Gen.Int32().Between(0, 10).WithBias(GC.Gen.Bias.None);
                    var gen = GC.Gen.Function<object, object, int>(returnGen);

                    var func = gen.SampleOne(seed: 0);

                    Assert.True(variable1.Select(arg1 => func(arg0, arg1)).Distinct().Count() > 1);
                });

                return test.When(variable1.Distinct().Count() >= 10);
            }
        }

        public class TernaryFunction
        {
            [Property]
            public void WhenTheThirdArgumentIsControlled_ItBehavesLikeTheBinaryFunction(object arg0, object arg1)
            {
                TestWithSeed(seed =>
                {
                    var returnGen = GC.Gen.Int32().Between(0, 10).WithBias(GC.Gen.Bias.None);

                    var gen0 = GC.Gen.Function<object, object, int>(returnGen);
                    var gen1 = GC.Gen.Function<object, object, object, int>(returnGen);

                    var func0 = gen0.SampleOne(seed: seed);
                    var func1 = gen1.SampleOne(seed: seed);

                    Assert.Equal(func0(arg0, arg1), func1(arg0, arg1, ControlArg));
                });
            }

            [Property]
            public FsCheck.Property ItIsEffectedByItsThirdArgument(object arg0, object arg1, List<object> variable2)
            {
                Action test = () => TestWithSeed(seed =>
                {
                    var returnGen = GC.Gen.Int32().Between(0, 10).WithBias(GC.Gen.Bias.None);
                    var gen = GC.Gen.Function<object, object, object, int>(returnGen);

                    var func = gen.SampleOne(seed: 0);

                    Assert.True(variable2.Select(arg2 => func(arg0, arg1, arg2)).Distinct().Count() > 1);
                });

                return test.When(variable2.Distinct().Count() >= 10);
            }
        }

        public class QuaternaryFunction
        {
            [Property]
            public void WhenTheFourthArgumentIsControlled_ItBehavesLikeTheTernaryFunction(object arg0, object arg1, object arg2)
            {
                TestWithSeed(seed =>
                {
                    var returnGen = GC.Gen.Int32().Between(0, 10).WithBias(GC.Gen.Bias.None);

                    var gen0 = GC.Gen.Function<object, object, object, int>(returnGen);
                    var gen1 = GC.Gen.Function<object, object, object, object, int>(returnGen);

                    var func0 = gen0.SampleOne(seed: seed);
                    var func1 = gen1.SampleOne(seed: seed);

                    Assert.Equal(func0(arg0, arg1, arg2), func1(arg0, arg1, arg2, ControlArg));
                });
            }

            [Property]
            public FsCheck.Property ItIsEffectedByItsFourthArgument(object arg0, object arg1, object arg2, List<object> variable3)
            {
                Action test = () => TestWithSeed(seed =>
                {
                    var returnGen = GC.Gen.Int32().Between(0, 10).WithBias(GC.Gen.Bias.None);
                    var gen = GC.Gen.Function<object, object, object, object, int>(returnGen);

                    var func = gen.SampleOne(seed: 0);

                    Assert.True(variable3.Select(arg3 => func(arg0, arg1, arg2, arg3)).Distinct().Count() > 1);
                });

                return test.When(variable3.Distinct().Count() >= 10);
            }
        }
    }
}
