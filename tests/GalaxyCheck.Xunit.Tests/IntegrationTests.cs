using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xunit;

namespace Tests
{
    public class IntegrationTests : IClassFixture<IntegrationTests.TestSuiteFixture>
    {
        private readonly TestSuiteFixture _fixture;

        public IntegrationTests(TestSuiteFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void InfallibleVoidProperty()
        {
            var testResult = _fixture.FindTestResult(nameof(InfallibleVoidProperty));

            testResult.Outcome.Should().Be("Passed");
        }

        [Fact]
        public void FallibleVoidProperty()
        {
            var testResult = _fixture.FindTestResult(nameof(FallibleVoidProperty));

            testResult.Outcome.Should().Be("Failed");
        }

        public record Invocation(object[] InjectedParameters);

        public record TestResult(string TestName, string Outcome, ImmutableList<Invocation> Invocations);

        public class TestSuiteFixture : IAsyncLifetime
        {
            private static readonly string CurrentDirectory = Directory.GetCurrentDirectory();
            private static readonly string TestReportFileName = Path.Combine(CurrentDirectory, $"test_result_{DateTime.UtcNow.Ticks}.trx");

            public ImmutableList<TestResult> TestResults { get; private set; } = default!;

            public async Task InitializeAsync()
            {
                var currentProjectDir = Enumerable
                    .Range(0, 3)
                    .Aggregate(new DirectoryInfo(CurrentDirectory), (acc, _) => acc.Parent!);
                var integrationProjectDir = Path.Combine(currentProjectDir.FullName, "IntegrationSample");

                var testProcess = Process.Start(new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"test {integrationProjectDir} --logger \"trx;LogFileName={TestReportFileName}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                });

                var standardOutput = await testProcess!.StandardOutput.ReadToEndAsync();
                var standardError = await testProcess!.StandardError.ReadToEndAsync();

                var testReport = File.ReadAllText(TestReportFileName);

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
                return new TestResult(
                    attributes.Single(a => a.Name.LocalName.Contains("testName")).Value.Split('.').Last(),
                    attributes.Single(a => a.Name.LocalName.Contains("outcome")).Value,
                    new List<Invocation>().ToImmutableList());
            }
        }
    }
}
