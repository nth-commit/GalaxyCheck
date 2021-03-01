using System;
using System.Linq;
using System.Reflection;

namespace GalaxyCheck
{
    public static class PropertyFactory
    {
        public static Property<object[]> ToProperty(this MethodInfo methodInfo, object? target)
        {
            return methodInfo.ReturnType == typeof(void)
                ? ToVoidProperty(methodInfo, target)
                : methodInfo.ReturnType == typeof(bool)
                ? ToBooleanProperty(methodInfo, target)
                : IsProperty(methodInfo.ReturnType)
                ? ToNestedProperty(methodInfo, target)
                : throw new Exception("Fatal: Unhandled return type");
        }

        private static Property<object[]> ToVoidProperty(MethodInfo methodInfo, object? target)
        {
            return Gen.Parameters(methodInfo).ForAll(parameters =>
            {
                try
                {
                    methodInfo.Invoke(target, parameters);
                }
                catch (TargetInvocationException ex)
                {
                    throw ex.InnerException;
                }
            });
        }

        private static Property<object[]> ToBooleanProperty(MethodInfo methodInfo, object? target)
        {
            return Gen.Parameters(methodInfo).ForAll(parameters =>
            {
                try
                {
                    return (bool)methodInfo.Invoke(target, parameters);
                }
                catch (TargetInvocationException ex)
                {
                    throw ex.InnerException;
                }
            });
        }

        private static bool IsProperty(Type type)
        {
            return type == typeof(Property);
        }

        private static Property<object[]> ToNestedProperty(MethodInfo methodInfo, object? target)
        {
            var gen =
                from parameters in Gen.Parameters(methodInfo)
                let property = (Property)methodInfo.Invoke(target, parameters)
                from propertyIteration in property
                select new Property<object[]>.Iteration(
                    (x) => propertyIteration.Func(x.Last()),
                    parameters.Append(propertyIteration.Input).ToArray());

            return new Property<object[]>(gen);
        }
    }
}
