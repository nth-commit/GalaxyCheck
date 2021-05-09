using GalaxyCheck;
using GalaxyCheck.Gens;
using GalaxyCheck.Gens.Injection.Int32;
using Newtonsoft.Json;
using System;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationSample
{
    public class Tests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public Tests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Property]
        public IGen<Test> InfalliblePureProperty() =>
            from a in Gen.Int32().Between(0, 100)
            select Property.ForThese(() =>
            {
                AnnounceTestInvocation(nameof(InfalliblePureProperty));
            });

        [Property]
        public IGen<Test> FalliblePureProperty() =>
            from a in Gen.Int32().Between(0, 100)
            select Property.ForThese(() =>
            {
                AnnounceTestInvocation(nameof(FalliblePureProperty));
                throw new Exception("Failed!");
            });

        [Property]
        public void InfallibleVoidProperty([Between(0, 100)] int x)
        {
            AnnounceTestInvocation(nameof(InfallibleVoidProperty));
        }

        [Property]
        public void FallibleVoidProperty([Between(0, 100)] int x)
        {
            AnnounceTestInvocation(nameof(FallibleVoidProperty));
            throw new Exception("Failed!");
        }

        [Property]
        public void InfallibleBooleanProperty([Between(0, 100)] int x)
        {
            AnnounceTestInvocation(nameof(InfallibleBooleanProperty));
        }

        [Property]
        public void FallibleBooleanProperty([Between(0, 100)] int x)
        {
            AnnounceTestInvocation(nameof(FallibleBooleanProperty));
            throw new Exception("Failed!");
        }

        [Property]
        public Property InfallibleNestedProperty([Between(0, 100)] int x)
        {
            AnnounceTestInvocation(nameof(InfallibleNestedProperty));
            return Gen.Int32().Between(0, 100).ForAll(y => { });
        }

        [Property]
        public Property FallibleNestedProperty([Between(0, 100)] int x)
        {
            AnnounceTestInvocation(nameof(FallibleNestedProperty));
            return Gen.Int32().Between(0, 100).ForAll(y => { throw new Exception("Failed!"); });
        }

        public class GenFactoryWhereIntsAreNonNegativeAttribute : GenFactoryAttribute
        {
            public override IGenFactory Get => Gen.Factory().RegisterType(Gen.Int32().GreaterThanEqual(0));
        }

        [Property]
        [GenFactoryWhereIntsAreNonNegativeAttribute]
        public void APropertyOfTheseIntIsThatItIsNonNegative(int x)
        {
            AnnounceTestInvocation(nameof(APropertyOfTheseIntIsThatItIsNonNegative));
            Assert.True(x >= 0);
        }

        [Sample]
        public IGen<Test> Sample() =>
            from a in Gen.Int32().Between(0, 100)
            select Property.ForThese(() =>
            {
            });

        private void AnnounceTestInvocation(string testName, params object[] injectedValues)
        {
            _testOutputHelper.WriteLine(JsonConvert.SerializeObject(new
            {
                testName,
                injectedValues
            }));
        }
    }
}
