using System;
using System.Linq;
using System.Reflection;

namespace GalaxyCheck.Gens.ReflectedGenHelpers.ReflectedGenHandlers
{
    internal class NonDefaultConstructorReflectedGenHandler : IReflectedGenHandler
    {
        private readonly ContextualErrorFactory _errorFactory;

        public NonDefaultConstructorReflectedGenHandler(ContextualErrorFactory errorFactory)
        {
            _errorFactory = errorFactory;
        }

        public bool CanHandleGen(Type type, ReflectedGenHandlerContext context) =>
            TryFindConstructor(type) != null;

        public IGen CreateGen(IReflectedGenHandler innerHandler, Type type, ReflectedGenHandlerContext context)
        {
            var methodInfo = typeof(NonDefaultConstructorReflectedGenHandler).GetMethod(
                nameof(CreateGenGeneric),
                BindingFlags.Static | BindingFlags.NonPublic)!;

            var genericMethodInfo = methodInfo.MakeGenericMethod(type);

            return (IGen)genericMethodInfo.Invoke(null!, new object[] { innerHandler, context, _errorFactory })!;
        }

        private static IGen<T> CreateGenGeneric<T>(IReflectedGenHandler innerHandler, ReflectedGenHandlerContext context, ContextualErrorFactory errorFactory)
        {
            var nullabilityInfoCtx = new NullabilityInfoContext();
            var constructor = TryFindConstructor(typeof(T))!;

            var parameterGens = constructor
                .GetParameters()
                .Select(parameter =>
                {
                    var nullabilityInfo = nullabilityInfoCtx.Create(parameter);
                    return innerHandler
                        .CreateGen(parameter.ParameterType, context.Next(parameter.Name ?? "<unknown>", parameter.ParameterType, nullabilityInfo)) // TODO: Indicate it's a ctor param in the path
                        .Cast<object>()
                        .MaybeNullableByNullabilityInfo(nullabilityInfo, parameter.ParameterType);
                });

            return Gen
                .Zip(parameterGens)
                .SelectMany(parameters =>
                {
                    // TODO: Try avoid SelectMany here
                    try
                    {
                        return Gen.Constant((T)constructor.Invoke(parameters.ToArray()));
                    }
                    catch (TargetInvocationException ex)
                    {
                        var innerEx = ex.InnerException;
                        var message = $"'{innerEx!.GetType()}' was thrown while calling constructor with message '{innerEx.Message}'";
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
