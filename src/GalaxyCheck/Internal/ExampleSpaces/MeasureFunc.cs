namespace GalaxyCheck.Internal.ExampleSpaces
{
    public delegate decimal MeasureFunc<T>(T value);

    public static class MeasureFunc
    {
        public static MeasureFunc<T> Unmeasured<T>() => (_) => 0;

        // TODO: Implement & test
        public static MeasureFunc<int> DistanceFromOrigin(int origin, int min, int max) => (value) => 0;
    }
}
