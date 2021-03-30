using System;
using System.Linq;
using System.Reflection;

namespace GalaxyCheck.Gens.AutoGenHelpers.AutoGenHandlers
{
    internal class NonDefaultConstructorAutoGenHandler : IAutoGenHandler
    {
        private readonly Func<string, AutoGenHandlerContext, IGen> _errorFactory;

        public NonDefaultConstructorAutoGenHandler(Func<string, AutoGenHandlerContext, IGen> errorFactory)
        {
            _errorFactory = errorFactory;
        }

        public bool CanHandleGen(Type type, AutoGenHandlerContext context) =>
            TryFindConstructor(type) != null;

        public IGen CreateGen(IAutoGenHandler innerHandler, Type type, AutoGenHandlerContext context)
        {
            var constructor = TryFindConstructor(type)!;

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
                        return Gen.Constant(constructor.Invoke(parameters.ToArray()));
                    }
                    catch (TargetInvocationException ex)
                    {
                        var innerEx = ex.InnerException;
                        var message = $"'{innerEx.GetType()}' was thrown while calling constructor with message '{innerEx.Message}'";
                        return _errorFactory(message, context).Cast<object>();
                    }
                });
        }

        private ConstructorInfo? TryFindConstructor(Type type) => (
            from constructor in type.GetConstructors()
            where constructor.IsPublic
            let parameters = constructor.GetParameters()
            where parameters.Length > 0
            orderby parameters.Length descending
            select constructor).FirstOrDefault();
    }
}
