namespace GalaxyCheck.Tests.TestUtility;

public class Spy<T>
{
    private readonly List<T> values = new();

    public void NotifyInvoked(T value)
    {
        values.Add(value);
    }

    public void Reset()
    {
        values.Clear();
    }

    public IReadOnlyList<T> Values => values;

    public int InvocationCount => values.Count;
}
