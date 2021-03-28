using GalaxyCheck.Gens;
using GalaxyCheck.Gens.Internal;
using GalaxyCheck.Gens.Iterations.Generic;
using GalaxyCheck.Gens.Parameters;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

/// <summary>
/// TODO:
///     - Docs
///     - Plugin-able strategies for handling nullables, tuples, ienumerables etc.
///     - Private properties
///     - Readonly properties
///     - Fields
///     - Exception handling in ctor/property
///     - Private default constructor
///     - Render ctor params in path more sensibly
/// </summary>
namespace GalaxyCheck
{
    public static partial class Gen
    {
        public static ICreateGen<T> Create<T>() => new CreateGen<T>();
    }
}

namespace GalaxyCheck.Gens
{
    public interface ICreateGen<T> : IGen<T>
    {
        ICreateGen<T> RegisterType<TField>(IGen<TField> fieldGen);

        ICreateGen<T> OverrideField<TField>(
            Expression<Func<T, TField>> fieldSelector,
            IGen<TField> fieldGen);
    }

    internal class CreateGen<T> : BaseGen<T>, ICreateGen<T>
    {
        private static readonly ImmutableDictionary<Type, IGen> DefaultRegisteredGensByType =
            new Dictionary<Type, IGen>
            {
                { typeof(int), Gen.Int32() },
                { typeof(char), Gen.Char() },
                { typeof(string), Gen.String() }
            }.ToImmutableDictionary();

        private readonly ImmutableDictionary<Type, IGen> _registeredGensByType;

        internal CreateGen(ImmutableDictionary<Type, IGen> registeredGensByType)
        {
            _registeredGensByType = registeredGensByType;
        }

        public CreateGen() : this(DefaultRegisteredGensByType)
        {
        }

        public ICreateGen<T> OverrideField<TField>(Expression<Func<T, TField>> fieldSelector, IGen<TField> fieldGen)
        {
            throw new NotImplementedException();
        }

        public ICreateGen<T> RegisterType<TField>(IGen<TField> fieldGen) =>
            new CreateGen<T>(_registeredGensByType.SetItem(typeof(TField), fieldGen));

        protected override IEnumerable<IGenIteration<T>> Run(GenParameters parameters) =>
            CreateGenHelpers
                .BuildGen(
                    typeof(T),
                    _registeredGensByType,
                    message => Gen.Advanced.Error<T>(nameof(CreateGen<T>), message),
                    ImmutableStack.Create<(string name, Type type)>(("$", typeof(T))))
                .Cast<T>()
                .Advanced.Run(parameters);
    }

    internal static class CreateGenHelpers
    {
        public static IGen BuildGen(
            Type type,
            ImmutableDictionary<Type, IGen> registeredGensByType,
            Func<string, IGen> errorFactory,
            ImmutableStack<(string name, Type type)> path)
        {
            var genFactoriesByPriority = new List<IGenFactory>
            {
                new RegistryGenFactory(registeredGensByType),
                new ListGenFactory(),
                new ConstructorParamsGenFactory(),
                new PropertySettingGenFactory()
            };

            var compositeGenFactory = new CompositeGenFactory(genFactoriesByPriority, errorFactory);

            return compositeGenFactory.CreateGen(compositeGenFactory, type, path);
        }

        private class RegistryGenFactory : IGenFactory
        {
            private readonly IReadOnlyDictionary<Type, IGen> _registeredGensByType;

            public RegistryGenFactory(IReadOnlyDictionary<Type, IGen> registeredGensByType)
            {
                _registeredGensByType = registeredGensByType;
            }

            public bool CanHandleType(Type type) => _registeredGensByType.ContainsKey(type);

            public IGen CreateGen(IGenFactory innerFactory, Type type, ImmutableStack<(string name, Type type)> path) => _registeredGensByType[type];
        }

        private class ListGenFactory : IGenFactory
        {
            public bool CanHandleType(Type type)
            {
                if (type.IsGenericType)
                {
                    var genericTypeDefinition = type.GetGenericTypeDefinition();
                    return GenMethodByGenericTypeDefinition.ContainsKey(genericTypeDefinition);
                }

                return false;
            }

            public IGen CreateGen(IGenFactory innerFactory, Type type, ImmutableStack<(string name, Type type)> path)
            {
                var elementType = type.GetGenericArguments().Single();
                var elementGen = innerFactory.CreateGen(elementType, path);

                var genericTypeDefinition = type.GetGenericTypeDefinition();
                var methodName = GenMethodByGenericTypeDefinition[genericTypeDefinition];

                var methodInfo = typeof(ListGenFactory).GetMethod(
                    methodName,
                    BindingFlags.Static | BindingFlags.NonPublic)!;

                var genericMethodInfo = methodInfo.MakeGenericMethod(elementType);

                return (IGen)genericMethodInfo.Invoke(null!, new object[] { elementGen });
            }

            private static readonly IReadOnlyDictionary<Type, string> GenMethodByGenericTypeDefinition = new Dictionary<Type, string>
            {
                { typeof(IReadOnlyList<>), nameof(CreateListGen) },
                { typeof(List<>), nameof(CreateListGen) },
                { typeof(ImmutableList<>), nameof(CreateImmutableListGen) },
                { typeof(IList<>), nameof(CreateListGen) },
            };

            private static IGen<List<T>> CreateListGen<T>(IGen<T> elementGen) => CreateImmutableListGen(elementGen).Select(x => x.ToList());

            private static IGen<ImmutableList<T>> CreateImmutableListGen<T>(IGen<T> elementGen) => elementGen.ListOf();
        }

        private class ConstructorParamsGenFactory : IGenFactory
        {
            public bool CanHandleType(Type type) => TryFindConstructor(type) != null;

            public IGen CreateGen(IGenFactory innerFactory, Type type, ImmutableStack<(string name, Type type)> path)
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

        private class PropertySettingGenFactory : IGenFactory
        {
            public bool CanHandleType(Type type) => type.BaseType == typeof(object);

            public IGen CreateGen(IGenFactory innerFactory, Type type, ImmutableStack<(string name, Type type)> path)
            {
                var setPropertyActionGens = type
                    .GetProperties()
                    .Select(property => innerFactory
                        .CreateGen(property.PropertyType, path.Push((property.Name, property.PropertyType)))
                        .Cast<object>()
                        .Select<object, Action<object>>(value => obj => property.SetValue(obj, value)));

                return Gen
                    .Zip(setPropertyActionGens)
                    .Select(setPropertyActions =>
                    {
                        var instance = Activator.CreateInstance(type);

                        foreach (var setPropertyAction in setPropertyActions)
                        {
                            setPropertyAction(instance);
                        }

                        return instance;
                    });
            }
        }

        private class CompositeGenFactory : IGenFactory
        {
            private readonly IReadOnlyList<IGenFactory> _genFactoriesByPriority;
            private readonly Func<string, IGen> _errorFactory;

            public CompositeGenFactory(
                IReadOnlyList<IGenFactory> genFactoriesByPriority,
                Func<string, IGen> errorFactory)
            {
                _genFactoriesByPriority = genFactoriesByPriority;
                _errorFactory = errorFactory;
            }

            public bool CanHandleType(Type type) =>
                _genFactoriesByPriority.Any(genFactory => genFactory.CanHandleType(type));

            public IGen CreateGen(IGenFactory innerFactory, Type type, ImmutableStack<(string name, Type type)> path)
            {
                if (path.Skip(1).Any(item => item.type == type))
                {
                    return _errorFactory($"detected circular reference on type '{type}'{RenderPathDiagnostics(path)}");
                }

                var gen = _genFactoriesByPriority
                    .Where(genFactory => genFactory.CanHandleType(type))
                    .Select(genFactory => genFactory.CreateGen(innerFactory, type, path))
                    .FirstOrDefault();

                if (gen == null)
                {
                    return _errorFactory($"could not resolve type '{type}'{RenderPathDiagnostics(path)}");
                }

                return gen;
            }

            private static string RenderPathDiagnostics(ImmutableStack<(string name, Type type)> path) =>
                path.Count() == 1 ? "" : $" at path '{RenderPath(path)}'";

            private static string RenderPath(ImmutableStack<(string name, Type type)> path) =>
                string.Join(".", path.Reverse().Select(item => item.name));
        }
    }

    internal interface IGenFactory
    {
        bool CanHandleType(Type type);

        IGen CreateGen(IGenFactory innerFactory, Type type, ImmutableStack<(string name, Type type)> path);
    }

    internal static class GenFactoryExtensions
    {
        public static IGen CreateGen(this IGenFactory innerFactory, Type type, ImmutableStack<(string name, Type type)> path) =>
            innerFactory.CreateGen(innerFactory, type, path);
    }
}
