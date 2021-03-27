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
///     - Plugin-able strategies for handling nullables, lists, tuples, ienumerables etc.
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
            if (path.Skip(1).Any(item => item.type == type))
            {
                return errorFactory($"detected circular reference on type '{type}'{RenderPathDiagnostics(path)}");
            }

            if (registeredGensByType.TryGetValue(type, out IGen? valueGen))
            {
                return valueGen;
            }

            if (type.BaseType == typeof(object))
            {
                return BuildObjectGen(type, registeredGensByType, errorFactory, path);
            }

            return errorFactory($"could not resolve type '{type}'{RenderPathDiagnostics(path)}");
        }

        private static IGen BuildObjectGen(
            Type type,
            ImmutableDictionary<Type, IGen> registeredGensByType,
            Func<string, IGen> errorFactory,
            ImmutableStack<(string name, Type type)> path)
        {
            var publicNonDefaultConstructor = type
                .GetConstructors()
                .Where(c => c.IsPublic)
                .Select(c => (constructor: c, parameters: c.GetParameters()))
                .Where(x => x.parameters.Length > 0)
                .OrderByDescending(x => x.parameters.Length)
                .Select(x => x.constructor)
                .FirstOrDefault();

            return publicNonDefaultConstructor == null
                ? BuildObjectGenFromProperties(type, registeredGensByType, errorFactory, path)
                : BuildObjectGenFromConstructor(registeredGensByType, errorFactory, path, publicNonDefaultConstructor);
        }

        private static IGen BuildObjectGenFromProperties(
            Type type,
            ImmutableDictionary<Type, IGen> registeredGensByType,
            Func<string, IGen> errorFactory,
            ImmutableStack<(string name, Type type)> path)
        {
            var setPropertiesGen = type.GetProperties().Aggregate(
                Gen.Constant<Action<object>>(_ => { }),
                (actionGen, property) => actionGen.SelectMany(setProperties =>
                {
                    var valueGen = BuildGen(
                        property.PropertyType,
                        registeredGensByType,
                        errorFactory,
                        path.Push((property.Name, property.PropertyType)));

                    return valueGen.Cast<object>().Select<object, Action<object>>(value => (obj) =>
                    {
                        setProperties(obj);
                        property.SetValue(obj, value);
                    });
                }));

            return setPropertiesGen.Select(setProperties =>
            {
                var instance = Activator.CreateInstance(type);
                setProperties(instance);
                return instance;
            });
        }

        private static IGen BuildObjectGenFromConstructor(
            ImmutableDictionary<Type, IGen> registeredGensByType,
            Func<string, IGen> errorFactory,
            ImmutableStack<(string name, Type type)> path,
            ConstructorInfo constructorInfo)
        {
            var parameterGens = constructorInfo.GetParameters().Select(parameter =>
            {
                var valueGen = BuildGen(
                    parameter.ParameterType,
                    registeredGensByType,
                    errorFactory,
                    path.Push((parameter.Name, parameter.ParameterType))); // TODO: Indicate it's a ctor param in the path

                return valueGen.Cast<object>();
            });

            return Gen
                .Zip(parameterGens)
                .Select(parameters => constructorInfo.Invoke(parameters.ToArray()));
        }

        private static string RenderPathDiagnostics(ImmutableStack<(string name, Type type)> path) =>
            path.Count() == 1 ? "" : $" at path '{RenderPath(path)}'";

        private static string RenderPath(ImmutableStack<(string name, Type type)> path) =>
            string.Join(".", path.Reverse().Select(item => item.name));
    }
}
