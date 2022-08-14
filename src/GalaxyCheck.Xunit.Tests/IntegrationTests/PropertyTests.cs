using FluentAssertions;
using Xunit;

namespace Tests.IntegrationTests
{
    public class PropertyTests : IClassFixture<PropertyTests.Fixture>
    {
        public class Fixture : TestSuiteFixture
        {
            public Fixture() : base("IntegrationSample")
            {
            }
        }

        private readonly Fixture _fixture;

        public PropertyTests(Fixture fixture)
        {
            _fixture = fixture;
        }

        [Theory]
        [InlineData("InfallibleVoidProperty")]
        [InlineData("InfallibleVoidPropertyAsync")]
        [InlineData("InfallibleBooleanProperty")]
        [InlineData("InfallibleBooleanPropertyAsync")]
        [InlineData("InfallibleReturnedProperty")]
        [InlineData("InfallibleReturnedPropertyAsync")]
        [InlineData("PropertyWithGenFromGenFactory")]
        [InlineData("PropertyWithGenFromGenFactoryAsync")]
        [InlineData("PropertyWithGenFromMemberGen")]
        [InlineData("PropertyWithGenFromMemberGenAsync")]
        public void PassingProperties(string testName)
        {
            var testResult = _fixture.FindTestResult(testName);

            testResult.Outcome.Should().Be("Passed");
        }

        [Theory]
        [InlineData("FallibleVoidProperty")]
        [InlineData("FallibleVoidPropertyAsync")]
        [InlineData("FallibleBooleanProperty")]
        [InlineData("FallibleBooleanPropertyAsync")]
        [InlineData("FallibleReturnedProperty")]
        [InlineData("FallibleReturnedPropertyAsync")]
        public void FailingProperties(string testName)
        {
            var testResult = _fixture.FindTestResult(testName);

            testResult.Outcome.Should().Be("Failed");
            testResult.Message.Should().StartWith("GalaxyCheck.Runners.PropertyFailedException");
        }

        [Theory]
        [InlineData("SampleVoidProperty")]
        [InlineData("SampleVoidPropertyAsync")]
        public void SampledProperties(string testName)
        {
            var testResult = _fixture.FindTestResult(testName);

            testResult.Outcome.Should().Be("Failed");
            testResult.Message.Should().StartWith("GalaxyCheck.SampleException : Test case failed to prevent false-positives.");
        }
    }
}
