using System;
using System.Collections.Immutable;

namespace GalaxyCheck.Gens.AutoGenHelpers
{
    internal interface IAutoGenFactory
    {
        bool CanHandleType(Type type);

        IGen CreateGen(IAutoGenFactory innerFactory, Type type, ImmutableStack<(string name, Type type)> path);
    }

    internal static class AutoGenFactoryExtensions
    {
        public static IGen CreateGen(this IAutoGenFactory innerFactory, Type type, ImmutableStack<(string name, Type type)> path) =>
            innerFactory.CreateGen(innerFactory, type, path);
    }
}
