using GalaxyCheck;
using GalaxyCheck.Runners.Replaying;

namespace GalaxyCheck_Tests_V3.Features.Replaying;

public class AboutReplaying
{
    [Fact]
    public void ItOnlyCallsTheTestFunctionOnce()
    {
        // Arrange
        var spy = new Spy<int>();
        var property = DummyProperties.EventuallyFalsified(spy);
        var counterexample = property.EnsureFalsified();
        spy.Reset();

        // Act
        property.Check(args => args with { Replay = counterexample.Replay });

        // Assert
        spy.Values
            .Should().ContainSingle()
            .Which.Should().Be(counterexample.Value);
    }

    [NebulaCheck.Property]
    public NebulaCheck.Property IfReplayIsAnInvalidFormat_ItThrows()
    {
        return NebulaCheck.Property.ForAll(FeatureGen.NotBase64(), DomainGen.Properties.Any(), Test);

        static void Test(string replay, PropertyProxy property)
        {
            // Act
            Action test = () => property.Check(args => args with { Replay = replay });

            // Assert
            test.Should()
                .Throw<Exceptions.GenErrorException>()
                .WithMessage("Error decoding replay string:*");
        }
    }

    [NebulaCheck.Property]
    public NebulaCheck.Property IfReplayEncodesAnInvalidPath_ItThrows()
    {
        return NebulaCheck.Property.ForAll(FeatureGen.Replay(allowEmptyPath: false), DomainGen.Properties.Nullary(), Test);

        static void Test(Replay replay, Property property)
        {
            // Act
            Action test = () => property.Check(args => args with { Replay = ReplayEncoding.Encode(replay) });

            // Assert
            test.Should()
                .Throw<Exceptions.GenErrorException>()
                .WithMessage("Error replaying last example, given replay string was no longer valid.*");
        }
    }

    [NebulaCheck.Property]
    public NebulaCheck.Property IfReplayEncodesAnError_ItThrows()
    {
        return NebulaCheck.Property.ForAll(FeatureGen.Replay(), DomainGen.Gens.Error().ToProperty(), Test);

        static void Test(Replay replay, Property property)
        {
            // Act
            Action test = () => property.Check(args => args with { Replay = ReplayEncoding.Encode(replay) });

            // Assert
            test.Should()
                .Throw<Exceptions.GenErrorException>()
                .WithMessage("Error during generation:*");
        }
    }
}
