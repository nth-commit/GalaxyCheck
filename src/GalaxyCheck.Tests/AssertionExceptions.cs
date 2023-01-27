using FluentAssertions.Specialized;

namespace Tests.V2
{
    public static class AssertionExceptions
    {
        public static ExceptionAssertions<GalaxyCheck.Exceptions.GenErrorException> WithGenErrorMessage(
            this ExceptionAssertions<GalaxyCheck.Exceptions.GenErrorException> assertions,
            string messageBody)
        {
            return assertions.WithMessage("Error during generation: " + messageBody);
        }
    }
}
