using System;
using System.Linq;
using Xunit;

using GalaxyCheck;
using DevGen = GalaxyCheck.Gen;

namespace Tests.V2.PropertyTests.AboutMethodInfoProperties
{
    /// <summary>
    /// TODO:
    ///     - Use different gen types as parameters when supported as parameters. Otherwise we are vulnerable to parameter
    ///       ordering bugs.
    /// </summary>
    public class AboutMethodInfoInput
    {
        [Fact]
        public void AVoidMethodInfoReceivesInputLikeForAll()
        {
            var gen0 = DevGen.Int32();
            var gen1 = DevGen.Int32();

            var forAllProperty = DevGen.ForAll(gen0, gen1, (x, y) => { });
            var forAllPropertyInput = forAllProperty.Select(x => new object[] { x.Input.Item1, x.Input.Item2 });

            Action<int, int> f = (x, y) => { };
            var methodInfoProperty = f.Method.ToProperty(null);
            var methodInfoPropertyInput = methodInfoProperty.Select(x => x.Input);

            GenAssert.Equal(forAllPropertyInput, methodInfoPropertyInput, 0, 10);
        }

        [Fact]
        public void ABooleanReturningMethodInfoReceivesInputLikeForAll()
        {
            var gen0 = DevGen.Int32();
            var gen1 = DevGen.Int32();

            var forAllProperty = DevGen.ForAll(gen0, gen1, (x, y) => true);
            var forAllPropertyInput = forAllProperty.Select(x => new object[] { x.Input.Item1, x.Input.Item2 });

            Func<int, int, bool> f = (x, y) => true;
            var methodInfoProperty = f.Method.ToProperty(null);
            var methodInfoPropertyInput = methodInfoProperty.Select(x => x.Input);

            GenAssert.Equal(forAllPropertyInput, methodInfoPropertyInput, 0, 10);
        }

        [Fact(Skip = "TODO")]
        public void APropertyReturningMethodInfoReceivesAConcatenationOfInput()
        {
            var gen0 = DevGen.Int32();
            var gen1 = DevGen.Int32();

            var forAllProperty = DevGen.ForAll(gen0, gen1, (x, y) => true);
            var forAllPropertyInput = forAllProperty.Select(x => new object[] { x.Input.Item1, x.Input.Item2 });

            Func<int, IProperty<int>> f = (x) => gen1.ForAll(y => true);
            var methodInfoProperty = f.Method.ToProperty(null);
            var methodInfoPropertyInput = methodInfoProperty.Select(x => x.Input);

            GenAssert.Equal(forAllPropertyInput, methodInfoPropertyInput, 0, 10);
        }
    }
}
