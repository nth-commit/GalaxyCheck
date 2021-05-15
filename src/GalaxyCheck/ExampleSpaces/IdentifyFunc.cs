namespace GalaxyCheck.ExampleSpaces
{
    internal delegate ExampleId IdentifyFunc<T>(T value);

    internal static class IdentifyFuncs
    {
        public static IdentifyFunc<T> Constant<T>() => (_) => ExampleId.Empty;

        public static IdentifyFunc<T> Default<T>() => (value) => ExampleId.Primitive(value);
    }
}
