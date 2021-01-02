namespace GalaxyCheck.Utility
{
    internal abstract record Option<T>
    {
    }

    internal static class Option
    {
        public record Some<T>(T Value) : Option<T>;

        public record None<T>() : Option<T>;
    }
}
