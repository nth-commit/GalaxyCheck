namespace GalaxyCheck.Abstractions
{
    public interface IProperty<T> : IGen<PropertyResult<T>>
    {
    }

    public abstract record PropertyResult<T>
    {
    }

    public static class PropertyResult
    {
        public sealed record Success<T>() : PropertyResult<T>;

        public sealed record Failure<T>() : PropertyResult<T>;
    }
}