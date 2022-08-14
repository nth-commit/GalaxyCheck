using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xunit;

namespace Tests.IntegrationTests
{
    public record Invocation(object[] InjectedParameters);

    public record TestResult(string TestName, string Outcome, string Message, ImmutableList<Invocation> Invocations);

    public abstract class TestSuiteFixture : IAsyncLifetime
    {
        private readonly string _currentDirectory = Directory.GetCurrentDirectory();
        private readonly string _testReportFileName;
        private readonly string _testSuiteName;

        public TestSuiteFixture(string testSuiteName)
        {
            _testReportFileName = Path.Combine(_currentDirectory, $"test_result_{testSuiteName}_{DateTime.UtcNow.Ticks}.trx");
            _testSuiteName = testSuiteName;
        }

        public ImmutableList<TestResult> TestResults { get; private set; } = default!;

        public async Task InitializeAsync()
        {
            var currentProjectDir = Enumerable
                .Range(0, 3)
                .Aggregate(new DirectoryInfo(_currentDirectory), (acc, _) => acc.Parent!);
            var integrationProjectDir = Path.Combine(currentProjectDir.FullName, "samples", _testSuiteName);

            var testProcess = Process.Start(new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"test {integrationProjectDir} --logger \"trx;LogFileName={_testReportFileName}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            });

            var standardOutput = await testProcess!.StandardOutput.ReadToEndAsync();
            var standardError = await testProcess!.StandardError.ReadToEndAsync();

            var testReport = File.ReadAllText(_testReportFileName);

            var testResults =
                from descendent in XElement.Parse(testReport).Descendants()
                where descendent.Name.LocalName == "UnitTestResult"
                select ElementToTestResult(descendent);

            TestResults = testResults.ToImmutableList();
        }

        public Task DisposeAsync() => Task.CompletedTask;

        public TestResult FindTestResult(string testName) => TestResults.Single(x => x.TestName == testName);

        private static TestResult ElementToTestResult(XElement element)
        {
            var attributes = element.Attributes();

            var message = (
                from desc in element.Descendants()
                where desc.Name.LocalName.Contains("Message")
                select desc.Value).SingleOrDefault();

            return new TestResult(
                attributes.Single(a => a.Name.LocalName.Contains("testName")).Value.Split('.').Last(),
                attributes.Single(a => a.Name.LocalName.Contains("outcome")).Value,
                message!,
                new List<Invocation>().ToImmutableList());
        }
    }
}
