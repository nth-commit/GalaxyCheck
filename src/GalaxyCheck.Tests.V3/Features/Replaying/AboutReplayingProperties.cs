using GalaxyCheck.Runners.Replaying;

namespace GalaxyCheck.Tests.Features.Replaying;

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

    [Stable.Property]
    public Stable.Property IfReplayIsAnInvalidFormat_ItThrows()
    {
        return Stable.Property.ForAll(FeatureGen.NotBase64(), DomainGen.Properties.Any(), Test);

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

    [Stable.Property]
    public Stable.Property IfReplayEncodesAnInvalidPath_ItThrows()
    {
        return Stable.Property.ForAll(FeatureGen.Replay(allowEmptyPath: false), DomainGen.Properties.Nullary(), Test);

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

    [Stable.Property]
    public Stable.Property IfReplayEncodesAnError_ItThrows()
    {
        return Stable.Property.ForAll(FeatureGen.Replay(), DomainGen.Gens.Error().ToProperty(), Test);

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
