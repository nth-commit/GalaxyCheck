using System.Diagnostics;

namespace GalaxyCheck.Internal.Utility
{
    internal abstract record Option<T>
    {
    }

    internal static class Option
    {
        //[DebuggerStepThrough]
        public record Some<T>(T Value) : Option<T>;

        //[DebuggerStepThrough]
        public record None<T>() : Option<T>;
    }
}
