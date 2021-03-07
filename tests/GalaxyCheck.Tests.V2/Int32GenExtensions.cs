using NebulaCheck.Gens;

namespace Tests.V2
{
    public static class Int32GenExtensions
    {
        public static IInt32Gen GreaterThan(this IInt32Gen gen, int comparator) => gen.GreaterThanEqual(comparator + 1);

        public static IInt32Gen LessThan(this IInt32Gen gen, int comparator) => gen.LessThanEqual(comparator - 1);
    }
}
