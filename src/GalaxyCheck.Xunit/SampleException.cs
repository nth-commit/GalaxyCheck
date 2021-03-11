using System;

namespace GalaxyCheck.Xunit
{
    public class SampleException : Exception
    {
        public SampleException(string message) : base($"Test case failed to prevent false-positives.{Environment.NewLine}{Environment.NewLine}{message}") { }
    }
}
