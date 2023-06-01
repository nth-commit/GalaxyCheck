using System;
using System.Linq;
using System.Reflection;

namespace GalaxyCheck.Gens.ReflectedGenHelpers.ReflectedGenHandlers
{
    internal class NonDefaultConstructorReflectedGenHandler : IReflectedGenHandler
    {
        public bool CanHandleGen(Type type, ReflectedGenHandlerContext context) =>
            TryFindConstructor(type) != null;

        public IGen CreateGen(IReflectedGenHandler innerHandler, Type type, ReflectedGenHandlerContext context)
        {
            var methodInfo = typeof(NonDefaultConstructorReflectedGenHandler).GetMethod(
                nameof(CreateGenGeneric),
                BindingFlags.Static | BindingFlags.NonPublic)!;

            var genericMethodInfo = methodInfo.MakeGenericMethod(type);

            return (IGen)genericMethodInfo.Invoke(null!, new object[] { innerHandler, context })!;
        }

        private static IGen<T> CreateGenGeneric<T>(IReflectedGenHandler innerHandler, ReflectedGenHandlerContext constructorContext)
        {
            var nullabilityInfoContext = new NullabilityInfoContext();
            var constructor = TryFindConstructor(typeof(T))!;

            var parameterGens = constructor
                .GetParameters()
                .Select(parameter =>
                {
                    // TODO: Indicate it's a ctor param in the path
                    var parameterContext = constructorContext.Next(parameter.Name ?? "<unknown>", parameter.ParameterType,
                        nullabilityInfoContext.Create(parameter));

                    return innerHandler
                        .CreateGen(parameter.ParameterType, parameterContext)
                        .Cast<object>()
                        .Advanced.ReferenceRngWaypoint(rngWaypoint => rngWaypoint.Influence(parameterContext.CalculateStableSeed()));
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
                        return constructorContext.Error<T>(message);
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
