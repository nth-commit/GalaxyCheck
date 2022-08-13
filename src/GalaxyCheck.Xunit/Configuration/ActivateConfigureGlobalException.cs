using System;

namespace GalaxyCheck.Configuration
{
    public class ActivateConfigureGlobalException : Exception
    {
        public ActivateConfigureGlobalException(Type type, Exception ex)
            : base(BuildMessage(type, ex), ex)
        {
        }

        private static string BuildMessage(Type type, Exception ex)
        {
            return @$"Failed to create instance of type '{type}' during global GalaxyCheck configuration.

Exception: {ex}";
        }
    }
}
