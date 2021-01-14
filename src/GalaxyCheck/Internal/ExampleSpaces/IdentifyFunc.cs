namespace GalaxyCheck.Internal.ExampleSpaces
{
    public delegate ExampleId IdentifyFunc<T>(T value);

    public static class IdentifyFuncs
    {
        public static IdentifyFunc<T> Constant<T>() => (_) => ExampleId.Empty;

        public static IdentifyFunc<T> Default<T>() => (value) => ExampleId.Primitive(value);
    }
}
