using System;
using System.Linq;
using System.Reflection;

namespace GalaxyCheck.Gens.AutoGenHelpers.AutoGenHandlers
{
    internal class NonDefaultConstructorAutoGenHandler : IAutoGenHandler
    {
        private readonly ContextualErrorFactory _errorFactory;

        public NonDefaultConstructorAutoGenHandler(ContextualErrorFactory errorFactory)
        {
            _errorFactory = errorFactory;
        }

        public bool CanHandleGen(Type type, AutoGenHandlerContext context) =>
            TryFindConstructor(type) != null;

        public IGen CreateGen(IAutoGenHandler innerHandler, Type type, AutoGenHandlerContext context)
        {
            var methodInfo = typeof(NonDefaultConstructorAutoGenHandler).GetMethod(
                nameof(CreateGenGeneric),
                BindingFlags.Static | BindingFlags.NonPublic)!;

            var genericMethodInfo = methodInfo.MakeGenericMethod(type);

            return (IGen)genericMethodInfo.Invoke(null!, new object[] { innerHandler, context, _errorFactory });
        }

        private static IGen<T> CreateGenGeneric<T>(IAutoGenHandler innerHandler, AutoGenHandlerContext context, ContextualErrorFactory errorFactory)
        {
            var constructor = TryFindConstructor(typeof(T))!;

            var parameterGens = constructor
                .GetParameters()
                .Select(parameter => innerHandler
                    .CreateGen(parameter.ParameterType, context.Next(parameter.Name, parameter.ParameterType)) // TODO: Indicate it's a ctor param in the path
                    .Cast<object>());

            return Gen
                .Zip(parameterGens)
                .SelectMany(parameters =>
                {
                    try
                    {
                        return Gen.Constant((T)constructor.Invoke(parameters.ToArray()));
                    }
                    catch (TargetInvocationException ex)
                    {
                        var innerEx = ex.InnerException;
                        var message = $"'{innerEx.GetType()}' was thrown while calling constructor with message '{innerEx.Message}'";
                        return errorFactory(message, context).Cast<T>();
                    }
                });
        }

        private static ConstructorInfo? TryFindConstructor(Type type) => (
            from constructor in type.GetConstructors()
            where constructor.IsPublic
            let parameters = constructor.GetParameters()
            where parameters.Length > 0
            orderby parameters.Length descending
            select constructor).FirstOrDefault();
    }
}
