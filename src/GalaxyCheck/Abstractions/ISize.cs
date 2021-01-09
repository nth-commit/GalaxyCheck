namespace GalaxyCheck.Abstractions
{
    public interface ISize
    {
        int Value { get; }

        ISize Increment();

        ISize BigIncrement();
    }
}
