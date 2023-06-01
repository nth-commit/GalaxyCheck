using FluentAssertions.Specialized;

namespace GalaxyCheck.Tests.TestUtility;

public static class AssertionExtensions
{
    public static ExceptionAssertions<Exceptions.GenErrorException> WithGenErrorMessage(
        this ExceptionAssertions<Exceptions.GenErrorException> assertions,
        string messageBody)
    {
        return assertions.WithMessage("Error during generation: " + messageBody);
    }
}
