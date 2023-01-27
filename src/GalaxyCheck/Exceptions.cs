using System;
using GalaxyCheck.Gens.Parameters;

namespace GalaxyCheck
{
    public static class Exceptions
    {
        public class GenErrorException : Exception
        {
            public GenParameters? ReplayParameters { get; }

            public GenErrorException(string message, GenParameters? replayParameters) : base(message)
            {
                ReplayParameters = replayParameters;
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
