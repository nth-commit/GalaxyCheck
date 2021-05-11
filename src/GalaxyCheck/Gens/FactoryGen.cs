/// <summary>
/// TODO:
///     - Docs
///     - Plugin-able strategies for handling nullables, ienumerables etc.
///     - Render ctor params in path more sensibly
/// </summary>
namespace GalaxyCheck
{
    using GalaxyCheck.Gens;

    public static partial class Gen
    {
        /// <summary>
        /// Creates a factory for <see cref="ITypedGen{T}"/>. The factory allows you to assign specific generators to
        /// types, and then be able to share that configuration across many auto-generators, see
        /// <see cref="IGenFactory"/>.
        /// </summary>
        /// <returns>A factory for auto-generators.</returns>
        public static IGenFactory Factory() => new GenFactory();
    }
}

namespace GalaxyCheck.Gens
{
    using GalaxyCheck.Gens.Internal;
    using GalaxyCheck.Gens.Iterations.Generic;
    using GalaxyCheck.Gens.Parameters;
    using GalaxyCheck.Gens.ReflectedGenHelpers;
    using GalaxyCheck.Internal;
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Linq.Expressions;

    public interface IReflectedGen<T> : IGen<T>
    {
        IReflectedGen<T> OverrideMember<TMember>(
            Expression<Func<T, TMember>> memberSelector,
            IGen<TMember> memberGen);
    }

    public interface IGenFactory
    {
        /// <summary>
        /// Registers a custom generator to the given type. The generator is applied to all <see cref="ITypedGen{T}"/>
        /// that are created by this factory.
        /// </summary>
        /// <param name="type">The type to register the generator against.</param>
        /// <param name="gen">The generator to register at the type.</param>
        /// <returns>A new factory with the registration applied.</returns>
        IGenFactory RegisterType(Type type, IGen gen);

        /// <summary>
        /// Registers a custom generator to the given type. The generator is applied to all <see cref="ITypedGen{T}"/>
        /// that are created by this factory.
        /// </summary>
        /// <param name="gen">The generator to register at the type.</param>
        /// <returns>A new factory with the registration applied.</returns>
        IGenFactory RegisterType<T>(IGen<T> gen);

        /// <summary>
        /// Creates an auto-generator for the given type, using the configuration that was specified on this factory.
        /// </summary>
        /// <returns>A generator for the given type.</returns>
        IReflectedGen<T> Create<T>();
    }

    internal class GenFactory : IGenFactory
    {
        private static readonly IReadOnlyDictionary<Type, IGen> DefaultRegisteredGensByType =
            new Dictionary<Type, IGen>
            {
                { typeof(short), Gen.Int16() },
                { typeof(int), Gen.Int32() },
                { typeof(char), Gen.Char() },
                { typeof(string), Gen.String() },
                { typeof(byte), Gen.Byte() }
            };

        private readonly ImmutableDictionary<Type, IGen> _registeredGensByType;

        private GenFactory(ImmutableDictionary<Type, IGen> registeredGensByType)
        {
            _registeredGensByType = registeredGensByType;
        }

        public GenFactory() : this(DefaultRegisteredGensByType.ToImmutableDictionary(kvp => kvp.Key, kvp => kvp.Value))
        {
        }

        public IGenFactory RegisterType(Type type, IGen gen) =>
            new GenFactory(_registeredGensByType.SetItem(type, gen));

        public IGenFactory RegisterType<T>(IGen<T> gen) =>
            RegisterType(typeof(T), gen);

        public IReflectedGen<T> Create<T>() => new ReflectedGen<T>(_registeredGensByType);
    }

    internal record ReflectedGenMemberOverride(string Path, IGen Gen);

    internal class ReflectedGen<T> : BaseGen<T>, IReflectedGen<T>
    {
        private readonly IReadOnlyDictionary<Type, IGen> _registeredGensByType;
        private readonly ImmutableList<ReflectedGenMemberOverride> _memberOverrides;
        private readonly string? _errorExpression;

        private ReflectedGen(
            IReadOnlyDictionary<Type, IGen> registeredGensByType,
            ImmutableList<ReflectedGenMemberOverride> memberOverrides,
            string? errorExpression)
        {
            _registeredGensByType = registeredGensByType;
            _memberOverrides = memberOverrides;
            _errorExpression = errorExpression;
        }

        public ReflectedGen(IReadOnlyDictionary<Type, IGen> registeredGensByType)
            : this(registeredGensByType, ImmutableList.Create<ReflectedGenMemberOverride>(), null)
        {
        }

        public IReflectedGen<T> OverrideMember<TMember>(Expression<Func<T, TMember>> memberSelector, IGen<TMember> fieldGen)
        {
            var pathResult = PathResolver.FromExpression(memberSelector);
            return pathResult.Match<string, string, IReflectedGen<T>>(
                fromLeft: path => new ReflectedGen<T>(
                    _registeredGensByType,
                    _memberOverrides.Add(new ReflectedGenMemberOverride(path, fieldGen)),
                    _errorExpression),
                fromRight: error => new ReflectedGen<T>(
                    _registeredGensByType,
                    _memberOverrides,
                    error));
        }

        protected override IEnumerable<IGenIteration<T>> Run(GenParameters parameters) =>
            BuildGen(_registeredGensByType, _memberOverrides, _errorExpression).Advanced.Run(parameters);

        private static IGen<T> BuildGen(
            IReadOnlyDictionary<Type, IGen> registeredGensByType,
            IReadOnlyList<ReflectedGenMemberOverride> memberOverrides,
            string? errorExpression)
        {
            if (errorExpression != null)
            {
                return Error(
                    $"expression '{errorExpression}' was invalid, an overridding expression may only contain member access");
            }

            var context = ReflectedGenHandlerContext.Create(typeof(T));
            return ReflectedGenBuilder
                .Build(typeof(T), registeredGensByType, memberOverrides, Error, context)
                .Cast<T>();
        }

        private static IGen<T> Error(string message) => Gen.Advanced.Error<T>(nameof(ReflectedGen<T>), message);
    }
}
