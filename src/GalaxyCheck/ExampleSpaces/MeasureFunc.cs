namespace GalaxyCheck.ExampleSpaces
{
    public delegate decimal MeasureFunc<T>(T value);

    public static class MeasureFunc
    {
        public static MeasureFunc<T> Unmeasured<T>() => (T _) => 0;

        // TODO: Implement & test
        public static MeasureFunc<int> DistanceFromOrigin(int origin, int min, int max) => (int value) => 0;
    }
}
