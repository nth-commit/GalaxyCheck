using FluentAssertions;
using Xunit;

namespace Tests.IntegrationTests
{
    public class GenSnapshotTestsSnapshooter : IClassFixture<GenSnapshotTestsSnapshooter.Fixture>
    {
        public class Fixture : TestSuiteFixture
        {
            public Fixture() : base("IntegrationSample_GenSnaphot_Snapshooter")
            {
            }
        }

        private readonly Fixture _fixture;

        public GenSnapshotTestsSnapshooter(Fixture fixture)
        {
            _fixture = fixture;
        }

        [Theory]
        [InlineData("TypedOutput")]
        [InlineData("ObjectOutput")]
        [InlineData("TaskOutput")]
        [InlineData("ValueTaskOuput")]
        public void Snapshots(string testName)
        {
            var testResult = _fixture.FindTestResult(testName);

            testResult.Outcome.Should().Be("Passed");
        }
    }
}
