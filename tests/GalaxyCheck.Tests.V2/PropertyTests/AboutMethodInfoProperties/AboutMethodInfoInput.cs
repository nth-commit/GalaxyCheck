using System;
using System.Linq;
using Xunit;
using GalaxyCheck;
using DevGen = GalaxyCheck.Gen;
using System.Reflection;

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
            DevGen.Select(forAllProperty, x => new object[] { x.Input.Item1, x.Input.Item2 });
            var forAllPropertyInput = forAllProperty.Select(x => new object[] { x.Input.Item1, x.Input.Item2 });

            Action<int, int> f = (x, y) => { };
            var methodInfoProperty = MethodProperty.Create(f.Method, null);
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
            var methodInfoProperty = MethodProperty.Create(f.Method, null);
            var methodInfoPropertyInput = methodInfoProperty.Select(x => x.Input);

            GenAssert.Equal(forAllPropertyInput, methodInfoPropertyInput, 0, 10);
        }

        private Property ARecursiveProperty(int x)
        {
            return DevGen.Int32().ForAll(y => true);
        }

        [Fact]
        public void APropertyReturningMethodInfoReceivesAConcatenationOfInput()
        {
            var gen0 = DevGen.Int32();
            var gen1 = DevGen.Int32();

            var forAllProperty = DevGen.ForAll(gen0, gen1, (x, y) => true);
            var forAllPropertyInput = forAllProperty.Select(x => new object[] { x.Input.Item1, x.Input.Item2 });

            var methodInfoProperty = MethodProperty.Create(GetMethod(nameof(ARecursiveProperty)), this);
            var methodInfoPropertyInput = methodInfoProperty.Select(x => x.Input);

            GenAssert.Equal(forAllPropertyInput, methodInfoPropertyInput, 0, 1);
        }

        private Property<int> AGenericRecursiveProperty(int x)
        {
            return DevGen.Int32().ForAll(y => true);
        }

        [Fact(Skip = "Not yet supported")]
        public void AGenericPropertyReturningMethodInfoReceivesAConcatenationOfInput()
        {
            var gen0 = DevGen.Int32();
            var gen1 = DevGen.Int32();

            var forAllProperty = DevGen.ForAll(gen0, gen1, (x, y) => true);
            var forAllPropertyInput = forAllProperty.Select(x => new object[] { x.Input.Item1, x.Input.Item2 });

            var methodInfoProperty = MethodProperty.Create(GetMethod(nameof(AGenericRecursiveProperty)), this);
            var methodInfoPropertyInput = methodInfoProperty.Select(x => x.Input);

            GenAssert.Equal(forAllPropertyInput, methodInfoPropertyInput, 0, 1);
        }

        private static MethodInfo GetMethod(string name)
        {
            var methodInfo = typeof(AboutMethodInfoInput).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance);

            if (methodInfo == null)
            {
                throw new Exception("Unable to locate method");
            }

            return methodInfo;
        }
    }
}
