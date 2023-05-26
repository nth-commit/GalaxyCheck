namespace Tests.IntegrationTests
{
    public class SnapshooterGenSnapshotTests : BaseTestSuiteTests<SnapshooterGenSnapshotTests.Fixture>
    {
        public class Fixture : TestSuiteFixture
        {
            public Fixture() : base("IntegrationSample_GenSnapshot_Snapshooter")
            {
            }
        }

        public SnapshooterGenSnapshotTests(Fixture fixture) : base(fixture)
        {
        }
    }
}
