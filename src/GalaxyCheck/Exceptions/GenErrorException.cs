using System;

namespace GalaxyCheck.Exceptions
{
    public class GenErrorException : Exception
    {
        public GenErrorException(string genName, string message)
            : base($"Error while running generator {genName}: {message}")
        {
        }
    }
}
