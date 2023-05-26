using Xunit;

namespace GalaxyCheck.Samples.EverythingIsSerializable;

public static class XunitExtensions
{
    public static TheoryData<T> ToTheoryData<T>(this IEnumerable<T> enumerable)
    {
        var theoryData = new TheoryData<T>();

        foreach (var item in enumerable)
        {
            theoryData.Add(item);
        }

        return theoryData;
    }
}
