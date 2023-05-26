using Snapshooter;
using Snapshooter.Xunit;
using Xunit;

namespace Tests.IntegrationTests;

public abstract class BaseTestSuiteTests<Fixture> : IClassFixture<Fixture>
    where Fixture : TestSuiteFixture
{
    private readonly Fixture _fixture;

    protected BaseTestSuiteTests(Fixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void TestNameSnapshots()
    {
        Snapshot.Match(_fixture.TestNameToTestResult.Keys, SnapshotNameExtension.Create(_fixture.TestSuiteName));
    }

    [Fact]
    public void TestOutputSnapshots()
    {
        foreach (var (testName, testOutput) in _fixture.TestNameToTestResult)
        {
            Snapshot.Match(testOutput, SnapshotNameExtension.Create(_fixture.TestSuiteName, testName));
        }
    }
}
