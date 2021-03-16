using FsCheck;
using FsCheck.Xunit;
using System.Linq;
using Xunit;
using GalaxyCheck;
using GC = GalaxyCheck;
using static Tests.TestUtils;

namespace Tests.Gen.Function
{
    [Properties(Arbitrary = new[] { typeof(ArbitraryFunctionOutput) }, MaxTest = 10)]
    public class AboutShrinking
    {
        public record FunctionOutput(int Value);

        public class ArbitraryFunctionOutput
        {
            public static Arbitrary<FunctionOutput> FunctionOutput() => FsCheck.Gen.Choose(0, 10).Select(x => new FunctionOutput(x)).ToArbitrary();
        }

        [Property]
        public void ItShrinksToTheFunctionWhereTheOutputIsX(FunctionOutput y)
        {
            TestWithSeed(seed =>
            {
                var returnGen = GC.Gen.Int32().Between(0, 10).WithBias(GC.Gen.Bias.None);
                var gen = GC.Gen.Function(returnGen);

                var func = gen.Minimal(seed: seed, pred: (f) => f() == y.Value);

                Assert.Equal(y.Value, func());
            });
        }

        [Property]
        public void IfAnyOutputOfTheFunctionIsX_ItShrinksToTheFunctionWhereAllTheOutputIsX(FunctionOutput y)
        {
            TestWithSeed(seed =>
            {
                var returnGen = GC.Gen.Int32().Between(0, 10).WithBias(GC.Gen.Bias.None);
                var gen = GC.Gen.Function<int, int>(returnGen);

                var inputs = Enumerable.Range(0, 100);
                var func = gen.Minimal(
                    seed: seed,
                    deepMinimal: true,
                    pred: (f) => inputs.Select(x => f(x)).Any(y0 => y0 == y.Value));

                Assert.All(inputs, x => Assert.Equal(y.Value, func(x)));
            });
        }
    }
}
