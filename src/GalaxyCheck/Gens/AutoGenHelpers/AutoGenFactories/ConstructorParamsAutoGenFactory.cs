using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace GalaxyCheck.Gens.AutoGenHelpers.AutoGenFactories
{
    internal class ConstructorParamsAutoGenFactory : IAutoGenFactory
    {
        public bool CanHandleType(Type type) => TryFindConstructor(type) != null;

        public IGen CreateGen(IAutoGenFactory innerFactory, Type type, ImmutableStack<(string name, Type type)> path)
        {
            var constructor = TryFindConstructor(type)!;

            var parameterGens = constructor
                .GetParameters()
                .Select(parameter => innerFactory
                    .CreateGen(parameter.ParameterType, path.Push((parameter.Name, parameter.ParameterType))) // TODO: Indicate it's a ctor param in the path
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
