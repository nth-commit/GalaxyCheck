using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xunit;

namespace Tests.IntegrationTests
{
    public record Invocation(object[] InjectedParameters);

    public record TestResult(string TestName, string Outcome, string Message, string Output);

    public abstract class TestSuiteFixture : IAsyncLifetime
    {
        private static readonly SemaphoreSlim Lock = new(1, 1);

        private readonly string _currentDirectory = Directory.GetCurrentDirectory();
        private readonly string _testReportFileName;

        public TestSuiteFixture(string testSuiteName)
        {
            TestSuiteName = testSuiteName;
            _testReportFileName = Path.Combine(_currentDirectory, $"test_result_{testSuiteName}_{DateTime.UtcNow.Ticks}.trx");
        }

        public string TestSuiteName { get; }

        public ImmutableList<TestResult> TestResults { get; private set; } = default!;

        public IReadOnlyDictionary<string, string> TestNameToTestResult { get; private set; } = default!;

        public string TestReport { get; private set; } = default!;

        public async Task InitializeAsync()
        {
            await Lock.WaitAsync();

            try
            {
                var currentProjectDir = Enumerable
                    .Range(0, 3)
                    .Aggregate(new DirectoryInfo(_currentDirectory), (acc, _) => acc.Parent!);
                var integrationProjectDir = Path.Combine(currentProjectDir.FullName, "samples", TestSuiteName);

                var testProcess = Process.Start(new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"test {integrationProjectDir} --logger \"trx;LogFileName={_testReportFileName}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                });

                var standardOutput = await testProcess!.StandardOutput.ReadToEndAsync();
                var standardError = await testProcess!.StandardError.ReadToEndAsync();

                TestReport = await File.ReadAllTextAsync(_testReportFileName);

                var testResults =
                    from descendent in XElement.Parse(TestReport).Descendants()
                    where descendent.Name.LocalName == "UnitTestResult"
                    select ElementToTestResult(descendent);

                TestResults = testResults.ToImmutableList();

                TestNameToTestResult = TestResults
                    .OrderBy(x => x.TestName)
                    .GroupBy(x => x.TestName.Contains('(') ? x.TestName[..x.TestName.IndexOf('(')] : x.TestName)
                    .SelectMany(g => g.Select((x, i) => (TestName: $"{g.Key}_{i}", TestResult: x)))
                    .ToDictionary(
                        x => x.TestName,
                        x => string.Join("\n", $"Outcome: {x.TestResult.Outcome}", "", "Output:", "", x.TestResult.Output));
            }
            finally
            {
                Lock.Release();
            }
        }

        public Task DisposeAsync() => Task.CompletedTask;

        private static TestResult ElementToTestResult(XElement element)
        {
            var attributes = element.Attributes();

            var message = (
                from desc in element.Descendants()
                where desc.Name.LocalName.Contains("Message")
                select desc.Value).SingleOrDefault();

            var output = (
                from desc in element.Descendants()
                where desc.Name.LocalName.Contains("Output")
                select desc.Value).SingleOrDefault();
            if (output?.Contains("   at ") is true)
            {
                output = output[..output.IndexOf("   at ")]; // Scrub the stacktrace with local paths 😅
            }

            return new TestResult(
                attributes.Single(a => a.Name.LocalName.Contains("testName")).Value.Split('.').Last(),
                attributes.Single(a => a.Name.LocalName.Contains("outcome")).Value,
                message!,
                output!);
        }
    }
}
