namespace GalaxyCheck.Tests.Features.Properties.Aggregators.Assert;

public class IntegrationTests
{
    [Fact]
    public void UnaryProperty()
    {
        var property = Gen.Int32().ForAll(x => x < 1000);

        var action = () => property.Assert();

        action.Should().Throw<PropertyFailedException>().Which.Message.MatchSnapshot();
    }

    [Fact]
    public void BinaryProperty()
    {
        var gen =
            from x in Gen.Int32().LessThan(int.MaxValue)
            from y in Gen.Int32().GreaterThan(x)
            select (x, y);
        var property = gen.ForAll(input => input.x < 1000);

        var action = () => property.Assert();

        action.Should().Throw<PropertyFailedException>().Which.Message.MatchSnapshot();
    }
}
