using GalaxyCheck;
using System;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Tests.V2.PropertyTests.ReflectedPropertyTests
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
            var gen0 = Gen.Int32();
            var gen1 = Gen.Int32();

            var forAllProperty = Property.ForAll(gen0, gen1, (x, y) => { });
            var forAllPropertyInput = forAllProperty.Select(x => new object[] { x.Input.Item1, x.Input.Item2 });

            Action<int, int> f = (x, y) => { };
            var methodInfoProperty = Property.Reflect(f.Method, null);
            var methodInfoPropertyInput = methodInfoProperty.Select(x => x.Input);

            GenAssert.Equal(forAllPropertyInput, methodInfoPropertyInput, 0, 10);
        }

        [Fact]
        public void ABooleanReturningMethodInfoReceivesInputLikeForAll()
        {
            var gen0 = Gen.Int32();
            var gen1 = Gen.Int32();

            var forAllProperty = Property.ForAll(gen0, gen1, (x, y) => true);
            var forAllPropertyInput = forAllProperty.Select(x => new object[] { x.Input.Item1, x.Input.Item2 });

            Func<int, int, bool> f = (x, y) => true;
            var methodInfoProperty = Property.Reflect(f.Method, null);
            var methodInfoPropertyInput = methodInfoProperty.Select(x => x.Input);

            GenAssert.Equal(forAllPropertyInput, methodInfoPropertyInput, 0, 10);
        }

        private Property ARecursiveProperty(int x)
        {
            return Gen.Int32().ForAll(y => true);
        }

        [Fact]
        public void APropertyReturningMethodInfoReceivesAConcatenationOfInput()
        {
            var gen0 = Gen.Int32();
            var gen1 = Gen.Int32();

            var forAllProperty = Property.ForAll(gen0, gen1, (x, y) => true);
            var forAllPropertyInput = forAllProperty.Select(x => new object[] { x.Input.Item1, x.Input.Item2 });

            var methodInfoProperty = Property.Reflect(GetMethod(nameof(ARecursiveProperty)), this);
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
