using GalaxyCheck;
using System;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Tests.V2.PropertyTests.ReflectedPropertyTests
{
    public class AboutMethodInfoInput
    {
        [Fact]
        public void AVoidMethodInfoReceivesInputLikeForAll()
        {
            var gen0 = Gen.Int32();
            var gen1 = Gen.String();

            var forAllProperty = Property.ForAll(gen0, gen1, (x, y) => { });
            var forAllPropertyInput = forAllProperty.Select(x => new object[] { x.Input.Item1, x.Input.Item2 });

            Action<int, string> f = (x, y) => { };
            var methodInfoProperty = Property.Reflect(f.Method, null);
            var methodInfoPropertyInput = methodInfoProperty.Select(x => x.Input);

            GenAssert.Equal(forAllPropertyInput, methodInfoPropertyInput, 0, 10);
        }

        [Fact]
        public void ABooleanReturningMethodInfoReceivesInputLikeForAll()
        {
            var gen0 = Gen.Int32();
            var gen1 = Gen.String();

            var forAllProperty = Property.ForAll(gen0, gen1, (x, y) => true);
            var forAllPropertyInput = forAllProperty.Select(x => new object[] { x.Input.Item1, x.Input.Item2 });

            Func<int, string, bool> f = (x, y) => true;
            var methodInfoProperty = Property.Reflect(f.Method, null);
            var methodInfoPropertyInput = methodInfoProperty.Select(x => x.Input);

            GenAssert.Equal(forAllPropertyInput, methodInfoPropertyInput, 0, 10);
        }
    }
}
