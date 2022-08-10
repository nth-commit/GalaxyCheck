using GalaxyCheck;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Tests.V2.PropertyTests.ReflectedPropertyTests
{
    public class AboutMethodInfoInput
    {
        public class Sync
        { 
            [Fact(Skip = "No longer true, replace these tests")]
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

            [Fact(Skip = "No longer true, replace these tests")]
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

        public class Async
        {
            [Fact(Skip = "No longer true, replace these tests")]
            public void AVoidMethodInfoReceivesInputLikeForAll()
            {
                var gen0 = Gen.Int32();
                var gen1 = Gen.String();

                var forAllProperty = Property.ForAll(gen0, gen1, (x, y) => { });
                var forAllPropertyInput = forAllProperty.Select(x => new object[] { x.Input.Item1, x.Input.Item2 });

                Action<int, string> f = (x, y) => { };
                var methodInfoProperty = Property.ReflectAsync(f.Method, null);
                var methodInfoPropertyInput = methodInfoProperty.Select(x => x.Input);

                GenAssert.Equal(forAllPropertyInput, methodInfoPropertyInput, 0, 10);
            }

            [Fact(Skip = "No longer true, replace these tests")]
            public void ABooleanReturningMethodInfoReceivesInputLikeForAll()
            {
                var gen0 = Gen.Int32();
                var gen1 = Gen.String();

                var forAllProperty = Property.ForAll(gen0, gen1, (x, y) => true);
                var forAllPropertyInput = forAllProperty.Select(x => new object[] { x.Input.Item1, x.Input.Item2 });

                Func<int, string, bool> f = (x, y) => true;
                var methodInfoProperty = Property.ReflectAsync(f.Method, null);
                var methodInfoPropertyInput = methodInfoProperty.Select(x => x.Input);

                GenAssert.Equal(forAllPropertyInput, methodInfoPropertyInput, 0, 10);
            }

            [Fact(Skip = "No longer true, replace these tests")]
            public void ATaskReturningMethodInfoReceivesInputLikeForAll()
            {
                var gen0 = Gen.Int32();
                var gen1 = Gen.String();

                var forAllProperty = Property.ForAll(gen0, gen1, (x, y) => { });
                var forAllPropertyInput = forAllProperty.Select(x => new object[] { x.Input.Item1, x.Input.Item2 });

                Func<int, string, Task> f = (x, y) => Task.CompletedTask;
                var methodInfoProperty = Property.ReflectAsync(f.Method, null);
                var methodInfoPropertyInput = methodInfoProperty.Select(x => x.Input);

                GenAssert.Equal(forAllPropertyInput, methodInfoPropertyInput, 0, 10);
            }

            [Fact(Skip = "No longer true, replace these tests")]
            public void ABooleanTaskReturningMethodInfoReceivesInputLikeForAll()
            {
                var gen0 = Gen.Int32();
                var gen1 = Gen.String();

                var forAllProperty = Property.ForAll(gen0, gen1, (x, y) => true);
                var forAllPropertyInput = forAllProperty.Select(x => new object[] { x.Input.Item1, x.Input.Item2 });

                Func<int, string, Task<bool>> f = (x, y) => Task.FromResult(true);
                var methodInfoProperty = Property.ReflectAsync(f.Method, null);
                var methodInfoPropertyInput = methodInfoProperty.Select(x => x.Input);

                GenAssert.Equal(forAllPropertyInput, methodInfoPropertyInput, 0, 10);
            }
        }
    }
}
