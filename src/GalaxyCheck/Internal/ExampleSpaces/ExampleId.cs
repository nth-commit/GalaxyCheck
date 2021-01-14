namespace GalaxyCheck.Internal.ExampleSpaces
{
    public record ExampleId(int HashCode)
    {
        public static ExampleId Empty => new ExampleId(-1923861349);

        public static ExampleId Primitive(object? obj) => obj == null
            ? Empty
            : new ExampleId(obj.GetHashCode());

        public static ExampleId Combine(ExampleId left, ExampleId right)
        {
            if (left == Empty) return right;
            if (right == Empty) return left;
            return Primitive(left.GetHashCode() * -1521134295 + right.GetHashCode());
        }
    }
}
