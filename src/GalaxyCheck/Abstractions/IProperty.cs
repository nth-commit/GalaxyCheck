namespace GalaxyCheck.Abstractions
{
    public interface IProperty<T> : IGen<PropertyIteration<T>>
    {
    }

    public delegate bool PropertyFunc<T>(T value);

    public record PropertyIteration<T>
    {
        public PropertyFunc<T> Func { get; init; }

        public T Input { get; init; }

        public PropertyIteration(PropertyFunc<T> func, T input)
        {
            Func = func;
            Input = input;
        }
    }
}