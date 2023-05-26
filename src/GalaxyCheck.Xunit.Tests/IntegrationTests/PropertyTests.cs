namespace Tests.IntegrationTests
{
    public class PropertyTests : BaseTestSuiteTests<PropertyTests.Fixture>
    {
        public class Fixture : TestSuiteFixture
        {
            public Fixture() : base("IntegrationSample")
            {
            }
        }

        public PropertyTests(Fixture fixture) : base(fixture)
        {
        }
    }
}
