using GalaxyCheck.Gens;
using GalaxyCheck.Gens.AutoGenHelpers;
using GalaxyCheck.Gens.Internal;
using GalaxyCheck.Gens.Iterations.Generic;
using GalaxyCheck.Gens.Parameters;
using GalaxyCheck.Internal;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq.Expressions;

/// <summary>
/// TODO:
///     - Docs
///     - Plugin-able strategies for handling nullables, ienumerables etc.
///     - Render ctor params in path more sensibly
///     - Use in parameter gen
/// </summary>
namespace GalaxyCheck
{
    public static partial class Gen
    {
        /// <summary>
        /// Creates a factory for <see cref="IAutoGen{T}"/>. The factory allows you to assign specific generators to
        /// types, and then be able to share that configuration across many auto-generators, see
        /// <see cref="IAutoGenFactory"/>.
        /// </summary>
        /// <returns>A factory for auto-generators.</returns>
        public static IAutoGenFactory AutoFactory() => new AutoGenFactory();

        /// <summary>
        /// Generates instances of the given type, using the default <see cref="IAutoGenFactory"/>. The auto-generator
        /// can not be configured as precisely as more specialized generators can be, but it can create complex types
        /// with minimal configuration through reflection.
        /// </summary>
        /// <returns>A generator for the given type.</returns>
        public static IAutoGen<T> Auto<T>() => AutoFactory().Create<T>();
    }

    public static partial class Extensions
    {
        /// <summary>
        /// Registers a custom generator to the given type. The generator is applied to all <see cref="IAutoGen{T}"/>
        /// that are created by this factory.
        /// </summary>
        /// <param name="gen">The generator to register at the type.</param>
        /// <returns>A new factory with the registration applied.</returns>
        public static IAutoGenFactory RegisterType<T>(this IAutoGenFactory factory, IGen<T> gen)
        {
            return factory.RegisterType(typeof(T), gen);
        }
    }
}

namespace GalaxyCheck.Gens
{
    public interface IAutoGenFactory
    {
        /// <summary>
        /// Registers a custom generator to the given type. The generator is applied to all <see cref="IAutoGen{T}"/>
        /// that are created by this factory.
        /// </summary>
        /// <param name="type">The type to register the generator against.</param>
        /// <param name="gen">The generator to register at the type.</param>
        /// <returns>A new factory with the registration applied.</returns>
        IAutoGenFactory RegisterType(Type type, IGen gen);

        /// <summary>
        /// Creates an auto-generator for the given type, using the configuration that was specified on this factory.
        /// </summary>
        /// <returns>A generator for the given type.</returns>
        IAutoGen<T> Create<T>();
    }

    internal class AutoGenFactory : IAutoGenFactory
    {
        private static readonly ImmutableDictionary<Type, IGen> DefaultRegisteredGensByType =
            new Dictionary<Type, IGen>
            {
                { typeof(int), Gen.Int32() },
                { typeof(char), Gen.Char() },
                { typeof(string), Gen.String() }
            }.ToImmutableDictionary();

        private readonly ImmutableDictionary<Type, IGen> _registeredGensByType;

        private AutoGenFactory(ImmutableDictionary<Type, IGen> registeredGensByType)
        {
            _registeredGensByType = registeredGensByType;
        }

        public AutoGenFactory() : this(DefaultRegisteredGensByType)
        {
            Create<(int, int)>();
        }

        public IAutoGenFactory RegisterType(Type type, IGen gen) =>
            new AutoGenFactory(_registeredGensByType.SetItem(type, gen));

        public IAutoGen<T> Create<T>() => new AutoGen<T>(_registeredGensByType);
    }

    public interface IAutoGen<T> : IGen<T>
    {
        IAutoGen<T> OverrideMember<TMember>(
            Expression<Func<T, TMember>> memberSelector,
            IGen<TMember> memberGen);
    }

    internal record AutoGenMemberOverride(string Path, IGen Gen);

    internal class AutoGen<T> : BaseGen<T>, IAutoGen<T>
    {
        private readonly ImmutableDictionary<Type, IGen> _registeredGensByType;
        private readonly ImmutableList<AutoGenMemberOverride> _memberOverrides;
        private readonly string? _errorExpression;

        private AutoGen(
            ImmutableDictionary<Type, IGen> registeredGensByType,
            ImmutableList<AutoGenMemberOverride> memberOverrides,
            string? errorExpression)
        {
            _registeredGensByType = registeredGensByType;
            _memberOverrides = memberOverrides;
            _errorExpression = errorExpression;
        }

        public AutoGen(ImmutableDictionary<Type, IGen> registeredGensByType)
            : this(registeredGensByType, ImmutableList.Create<AutoGenMemberOverride>(), null)
        {
        }

        public IAutoGen<T> OverrideMember<TMember>(Expression<Func<T, TMember>> memberSelector, IGen<TMember> fieldGen)
        {
            var pathResult = PathResolver.FromExpression(memberSelector);
            return pathResult.Match<string, string, IAutoGen<T>>(
                fromLeft: path => new AutoGen<T>(
                    _registeredGensByType,
                    _memberOverrides.Add(new AutoGenMemberOverride(path, fieldGen)),
                    _errorExpression),
                fromRight: error => new AutoGen<T>(
                    _registeredGensByType,
                    _memberOverrides,
                    error));
        }

        protected override IEnumerable<IGenIteration<T>> Run(GenParameters parameters) =>
            BuildGen(_registeredGensByType, _memberOverrides, _errorExpression).Advanced.Run(parameters);

        private static IGen<T> BuildGen(
            IReadOnlyDictionary<Type, IGen> registeredGensByType,
            IReadOnlyList<AutoGenMemberOverride> memberOverrides,
            string? errorExpression)
        {
            if (errorExpression != null)
            {
                return Error($"expression '{errorExpression}' was invalid, an overridding expression may only contain member access");
            }

            var context = AutoGenHandlerContext.Create(typeof(T));
            return AutoGenBuilder
                .Build(typeof(T), registeredGensByType, memberOverrides, AutoGen<T>.Error, context)
                .Cast<T>();
        }

        private static IGen<T> Error(string message) => Gen.Advanced.Error<T>(nameof(AutoGen<T>), message);
    }
}
