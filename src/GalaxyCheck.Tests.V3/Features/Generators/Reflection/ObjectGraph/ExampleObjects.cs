namespace GalaxyCheck.Tests.Features.Generators.Reflection.ObjectGraph;

public static class ExampleObjects
{
    public class ClassWithFailingConstructor
    {
        private static readonly Exception _exception = new("Constructor failed");

        public ClassWithFailingConstructor()
        {
            throw _exception;
        }
    }

    public class ClassWithNestedFailingConstructor
    {
        public ClassWithFailingConstructor Property { get; set; } = null!;
    }

    public class ClassWithArrayOfFailingConstructors
    {
        public ClassWithFailingConstructor[] Property { get; set; } = null!;
    }

    public class ClassWithArrayOfNestedFailingConstructors
    {
        public ClassWithNestedFailingConstructor[] Property { get; set; } = null!;
    }

    public class ClassWithFailingConstructorAsConstructorParameter
    {
        public ClassWithFailingConstructor Property { get; set; }

        public ClassWithFailingConstructorAsConstructorParameter(ClassWithFailingConstructor property)
        {
            Property = property;
        }
    }

    public class ClassWithNestedFailingConstructorAsConstructorParameter
    {
        public ClassWithNestedFailingConstructor Property { get; set; }

        public ClassWithNestedFailingConstructorAsConstructorParameter(ClassWithNestedFailingConstructor property)
        {
            Property = property;
        }
    }

    public class ClassWithOneProperty
    {
        public int Property { get; set; }
    }

    public class ClassWithTwoProperties
    {
        public int Property1 { get; set; }
        public int Property2 { get; set; }
    }

    public class ClassWithOneNestedProperty
    {
        public ClassWithOneProperty Property { get; set; } = null!;
    }

    public abstract class UnconstructableClass
    {
    }
}
