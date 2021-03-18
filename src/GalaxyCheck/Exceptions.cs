using System;

namespace GalaxyCheck
{
    public static class Exceptions
    {
        public class GenErrorException : Exception
        {
            public GenErrorException(string message) : base(message)
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
