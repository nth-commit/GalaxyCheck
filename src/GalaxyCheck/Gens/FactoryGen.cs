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
        /// Registers a custom generator to the given type. The generator is applied to all <see cref="IReflectedGen{T}"/>
        /// that are created by this factory.
        /// </summary>
        /// <param name="type">The type to register the generator against.</param>
        /// <param name="gen">The generator to register at the type.</param>
        /// <returns>A new factory with the registration applied.</returns>
        IGenFactory RegisterType(Type type, IGen gen);

        /// <summary>
        /// Registers a custom generator to the given type. The generator is applied to all <see cref="IReflectedGen{T}"/>
        /// that are created by this factory.
        /// </summary>
        /// <param name="gen">The generator to register at the type.</param>
        /// <returns>A new factory with the registration applied.</returns>
        IGenFactory RegisterType<T>(IGen<T> gen);

        /// <summary>
        /// Registers a custom generator to the given type. The generator is applied to all
        /// <see cref="IReflectedGen{T}"/>. The registration function can reference existing conventions defined by the
        /// factory. Note, only conventions previously defined are used.
        /// </summary>
        /// <param name="type">The type to register the generator against.</param>
        /// <param name="genFunc">A function that takes the current factory and returns a generator. The factory can be
        /// used to create the gen, but it only uses conventions that were previously defined.</param>
        /// <returns>A new factory with the registration applied.</returns>
        IGenFactory RegisterType(Type type, Func<IGenFactory, IGen> genFunc);

        /// <summary>
        /// Registers a custom generator to the given type. The generator is applied to all
        /// <see cref="IReflectedGen{T}"/>. The registration function can reference existing conventions defined by the
        /// factory. Note, only conventions previously defined are used.
        /// </summary>
        /// <param name="genFunc">A function that takes the current factory and returns a generator. The factory can be
        /// used to create the gen, but it only uses conventions that were previously defined.</param>
        /// <returns>A new factory with the registration applied.</returns>
        IGenFactory RegisterType<T>(Func<IGenFactory, IGen<T>> genFunc);

        /// <summary>
        /// Creates an auto-generator for the given type, using the configuration that was specified on this factory.
        /// </summary>
        /// <returns>A generator for the given type.</returns>
        IReflectedGen<T> Create<T>();
    }

    internal class GenFactory : IGenFactory
    {
        private static readonly IReadOnlyDictionary<Type, Func<IGen>> DefaultRegisteredGensByType =
            new Dictionary<Type, Func<IGen>>
            {
                { typeof(short), () => Gen.Int16() },
                { typeof(ushort), () => Gen.Int16().Select(x => (ushort)x) },
                { typeof(int), () => Gen.Int32() },
                { typeof(uint), () => Gen.Int32().Select(x => (uint)x) },
                { typeof(long), () => Gen.Int64() },
                { typeof(ulong), () => Gen.Int64().Select(x => (ulong)x) },
                { typeof(char), () => Gen.Char() },
                { typeof(string), () => Gen.String() },
                { typeof(byte), () => Gen.Byte() },
                { typeof(Guid), () => Gen.Guid() },
                { typeof(DateTime), () => Gen.DateTime() },
                { typeof(bool), () => Gen.Boolean() }
            };

        private readonly ImmutableDictionary<Type, Func<IGen>> _registeredGensByType;

        private GenFactory(ImmutableDictionary<Type, Func<IGen>> registeredGensByType)
        {
            _registeredGensByType = registeredGensByType;
        }

        public GenFactory() : this(DefaultRegisteredGensByType
            .ToImmutableDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value))
        {
        }

        public IGenFactory RegisterType(Type type, IGen gen) =>
            RegisterType(type, (_) => gen);

        public IGenFactory RegisterType<T>(IGen<T> gen) =>
            RegisterType((_) => gen);

        public IGenFactory RegisterType(Type type, Func<IGenFactory, IGen> genFunc) =>
            new GenFactory(_registeredGensByType.SetItem(type, () => genFunc(this)));

        public IGenFactory RegisterType<T>(Func<IGenFactory, IGen<T>> genFunc) =>
            new GenFactory(_registeredGensByType.SetItem(typeof(T), () => genFunc(this)));

        public IReflectedGen<T> Create<T>() => new ReflectedGen<T>(_registeredGensByType);
    }

    internal record ReflectedGenMemberOverride(string Path, IGen Gen);

    internal class ReflectedGen<T> : BaseGen<T>, IReflectedGen<T>
    {
        private readonly IReadOnlyDictionary<Type, Func<IGen>> _registeredGensByType;
        private readonly ImmutableList<ReflectedGenMemberOverride> _memberOverrides;
        private readonly string? _errorExpression;

        private ReflectedGen(
            IReadOnlyDictionary<Type, Func<IGen>> registeredGensByType,
            ImmutableList<ReflectedGenMemberOverride> memberOverrides,
            string? errorExpression)
        {
            _registeredGensByType = registeredGensByType;
            _memberOverrides = memberOverrides;
            _errorExpression = errorExpression;
        }

        public ReflectedGen(IReadOnlyDictionary<Type, Func<IGen>> registeredGensByType)
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
            IReadOnlyDictionary<Type, Func<IGen>> registeredGensByType,
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
