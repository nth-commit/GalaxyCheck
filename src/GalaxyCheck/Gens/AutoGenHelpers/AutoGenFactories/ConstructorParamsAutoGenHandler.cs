using System;
using System.Linq;
using System.Reflection;

namespace GalaxyCheck.Gens.AutoGenHelpers.AutoGenFactories
{
    internal class ConstructorParamsAutoGenHandler : IAutoGenHandler
    {
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
                .Select(parameters => constructor.Invoke(parameters.ToArray()));
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
