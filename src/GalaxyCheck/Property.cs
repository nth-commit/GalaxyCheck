using System.Reflection;

namespace GalaxyCheck
{
    public static class Property
    {
        public static IProperty<object[]> ToProperty(this MethodInfo methodInfo, object? target)
        {
            return methodInfo.ReturnType == typeof(void)
                ? ToVoidProperty(methodInfo, target)
                : methodInfo.ReturnType == typeof(bool)
                ? ToBooleanProperty(methodInfo, target)
                : ThrowUnhandled<IProperty<object[]>>("Unhandled return type");
        }

        private static IProperty<object[]> ToVoidProperty(MethodInfo methodInfo, object? target)
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

        private static IProperty<object[]> ToBooleanProperty(MethodInfo methodInfo, object? target)
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

        private static T ThrowUnhandled<T>(string message)
        {
            throw new System.Exception(message);
        }
    }
}
