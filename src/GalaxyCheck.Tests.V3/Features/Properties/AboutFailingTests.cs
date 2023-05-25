namespace GalaxyCheck.Tests.Features.Properties;

public class AboutFailingTests
{
    [Fact]
    public void ItShouldFail()
    {
        // Arrange
        var property = Gen.Constant(true).ForAll(x => x == false);

        // Act
        var result = property.Check();

        // Assert
        result.Falsified.Should().BeTrue();
        result.Iterations.Should().Be(1);
    }
}
