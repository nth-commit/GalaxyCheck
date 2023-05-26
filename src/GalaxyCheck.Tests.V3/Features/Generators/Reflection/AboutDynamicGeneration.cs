namespace GalaxyCheck.Tests.Features.Generators.Reflection;

public class AboutDynamicGeneration
{
    public static TheoryData<Type> Types => new()
    {
        typeof(int),
        typeof(string)
    };

    [Theory]
    [MemberData(nameof(Types))]
    public void ItCanGenerateTheType(Type type)
    {
        // Arrange
        var gen = Gen.Create(type);

        // Act
        var sample = gen.Sample();

        // Assert
        sample.Should().AllBeAssignableTo(type);
    }
}
