using System;

namespace GalaxyCheck
{
    public static class Exceptions
    {
        public class GenErrorException : Exception
        {
            public GenErrorException(string genName, string message)
                : base($"Error while running generator {genName}: {message}")
            {
            }
        }

        public class GenExhaustionException : Exception
        {
        }

        public class NoMinimalFoundException : Exception
        {
        }

        public class GenLimitExceededException : Exception
        {
            public GenLimitExceededException(string message)
                : base(message)
            {
            }
        }
    }
}
