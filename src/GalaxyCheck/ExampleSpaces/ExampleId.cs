namespace GalaxyCheck.ExampleSpaces
{
    public abstract record ExampleId
    {
        private ExampleId() { }

        private record CaseEmpty() : ExampleId()
        {
            public override string ToString()
            {
                return "<empty>";
            }
        }

        private record CasePrimitive(long HashCode) : ExampleId()
        {
            public override string ToString()
            {
                return HashCode.ToString();
            }
        }

        public static ExampleId Empty => new CaseEmpty();

        public static ExampleId Primitive(long id) => new CasePrimitive(id);

        public static ExampleId Primitive(object? obj) => obj == null
            ? Empty
            : new CasePrimitive(obj.GetHashCode());

        public static ExampleId Combine(ExampleId left, ExampleId right)
        {
            return (left, right) switch
            {
                (CaseEmpty, var r) => r,
                (var l, CaseEmpty) => l,
                (CasePrimitive l, CasePrimitive r) => CombinePrimitives(l, r),
                _ => throw new System.Exception("Unhandled case")
            };
        }

        private static ExampleId CombinePrimitives(CasePrimitive l, CasePrimitive r)
        {
            unchecked
            {
                return Primitive(l.HashCode * -1521134295 + r.HashCode);
            }
        }
    }
}
