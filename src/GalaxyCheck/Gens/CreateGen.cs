using GalaxyCheck.Gens;
using GalaxyCheck.Gens.Internal;
using GalaxyCheck.Gens.Iterations.Generic;
using GalaxyCheck.Gens.Parameters;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;

/// <summary>
/// TODO:
///     - Docs
///     - Plugin-able strategies for handling nullables, lists, tuples, ienumerables, records etc.
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
                return BuildObjectGenerator(type, registeredGensByType, errorFactory, path);
            }

            return errorFactory($"could not resolve type '{type}'{RenderPathDiagnostics(path)}");
        }

        private static IGen BuildObjectGenerator(
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

                    if (valueGen == null)
                    {
                        var pathString = string.Join(".", path.Reverse().Select(item => item.name));
                        return errorFactory($"could not resolve type '{property.PropertyType}' at path '{pathString}'").Cast<Action<object>>();
                    }

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

        private static string RenderPathDiagnostics(ImmutableStack<(string name, Type type)> path) =>
            path.Count() == 1 ? "" : $" at path '{RenderPath(path)}'";

        private static string RenderPath(ImmutableStack<(string name, Type type)> path) =>
            string.Join(".", path.Reverse().Select(item => item.name));
    }
}
