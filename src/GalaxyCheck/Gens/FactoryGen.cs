namespace GalaxyCheck
{
    using Gens;

    public static partial class Gen
    {
        /// <summary>
        /// Creates a factory for <see cref="IReflectedGen{T}"/>. The factory allows you to assign specific generators to
        /// types, and then be able to share that configuration across many auto-generators, see
        /// <see cref="IGenFactory"/>.
        /// </summary>
        /// <returns>A factory for auto-generators.</returns>
        public static IGenFactory Factory() => new GenFactory();
    }
}

namespace GalaxyCheck.Gens
{
    using Internal;
    using ReflectedGenHelpers;
    using GalaxyCheck.Internal;
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Drawing;


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
        IReflectedGen<T> Create<T>(NullabilityInfo? nullabilityInfo = null) where T : notnull;
    }

    internal class GenFactory : IGenFactory
    {
        private static readonly IReadOnlyDictionary<Type, Func<IGen>> DefaultRegisteredGensByType =
            new Dictionary<Type, Func<IGen>>
            {
                // Integers and complements
                { typeof(short), () => Gen.Int16() },
                { typeof(ushort), () => Gen.Int16().Select(x => (ushort)x) },
                { typeof(int), () => Gen.Int32() },
                { typeof(uint), () => Gen.Int32().Select(x => (uint)x) },
                { typeof(long), () => Gen.Int64() },
                { typeof(ulong), () => Gen.Int64().Select(x => (ulong)x) },
                { typeof(byte), () => Gen.Byte() },
                { typeof(sbyte), () => Gen.Byte().Select(x => (sbyte)x) },

                // Dates and times
                { typeof(DateTime), () => Gen.DateTime() },
                {
                    typeof(DateTimeOffset),
                    () => Gen.DateTime().Within(new DateTime(1, 1, 2), new DateTime(9998, 12, 30))
                        .Select((x => new DateTimeOffset(x)))
                },
                { typeof(TimeSpan), () => Gen.Int64().Select(x => new TimeSpan(x)) },

                // Decimals - not interesting values, but won't halt the generator at least
                { typeof(float), () => Gen.Int32().Select(x => (float)x) },
                { typeof(double), () => Gen.Int32().Select(x => (double)x) },
                { typeof(decimal), () => Gen.Int32().Select(x => (decimal)x) },

                // Misc...
                { typeof(char), () => Gen.Char() },
                { typeof(string), () => Gen.String() },
                { typeof(Guid), () => Gen.Guid() },
                { typeof(bool), () => Gen.Boolean() },
                { typeof(Color), () => Gen.Int32().Select(x => Color.FromArgb(x)) },
                { typeof(Uri), () => Gen.Constant(new Uri("https://github.com/nth-commit/GalaxyCheck/releases")) }
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

        IGenFactory IGenFactory.RegisterType(Type type, IGen gen) => RegisterType(type, (_) => gen);

        IGenFactory IGenFactory.RegisterType<T>(IGen<T> gen) => RegisterType(typeof(T), (_) => gen);

        IGenFactory IGenFactory.RegisterType(Type type, Func<IGenFactory, IGen> genFunc) => RegisterType(type, genFunc);

        IGenFactory IGenFactory.RegisterType<T>(Func<IGenFactory, IGen<T>> genFunc) => RegisterType(typeof(T), genFunc);

        //// TODO: Cache this call by (this/T)
        IReflectedGen<T> IGenFactory.Create<T>(NullabilityInfo? nullabilityInfo) =>
            new ReflectedGen<T>(_registeredGensByType, nullabilityInfo);

        private IGenFactory RegisterType(Type type, Func<IGenFactory, IGen> genFunc) =>
            new GenFactory(_registeredGensByType.SetItem(type, () => genFunc(this)));
    }

    internal record ReflectedGenMemberOverride(string Path, IGen Gen);

    internal record ReflectedGen<T>(
        IReadOnlyDictionary<Type, Func<IGen>> RegisteredGensByType,
        ImmutableList<ReflectedGenMemberOverride> MemberOverrides,
        ReflectedGen<T>.OverrideMemberErrorKind? OverrideMemberError,
        NullabilityInfo? NullabilityInfo) : GenProvider<T>, IReflectedGen<T>
    {
        public record OverrideMemberErrorKind
        {
            private OverrideMemberErrorKind()
            {
            }

            public record InvalidExpression(string Expression) : OverrideMemberErrorKind;

            public record AttemptedOverrideOnRegisteredGen(string Expression) : OverrideMemberErrorKind;
        }

        public ReflectedGen(
            IReadOnlyDictionary<Type, Func<IGen>> registeredGensByType,
            NullabilityInfo? nullabilityInfo)
            : this(
                registeredGensByType,
                ImmutableList.Create<ReflectedGenMemberOverride>(),
                null,
                nullabilityInfo)
        {
        }

        public IReflectedGen<T> OverrideMember<TMember>(Expression<Func<T, TMember>> memberSelector,
            IGen<TMember> fieldGen)
        {
            // TODO: Either.Select, Either.SelectMany, Either.SelectError
            var pathResult = PathResolver.FromExpression(memberSelector);
            return pathResult.Match<(string path, string expression), string, IReflectedGen<T>>(
                fromLeft: x => RegisteredGensByType.ContainsKey(typeof(T))
                    ? this with
                    {
                        OverrideMemberError = new OverrideMemberErrorKind.AttemptedOverrideOnRegisteredGen(x.expression)
                    }
                    : this with
                    {
                        MemberOverrides = MemberOverrides.Add(new ReflectedGenMemberOverride(x.path, fieldGen))
                    },
                fromRight: expression => this with
                {
                    OverrideMemberError = new OverrideMemberErrorKind.InvalidExpression(expression)
                });
        }

        protected override IGen<T> Get
        {
            get
            {
                if (OverrideMemberError != null)
                {
                    return ResolveError(OverrideMemberError);
                }

                var context = ReflectedGenHandlerContext.Create(typeof(T), NullabilityInfo);

                return ReflectedGenBuilder
                    .Build(typeof(T), RegisteredGensByType, MemberOverrides, Error, context)
                    .Cast<T>();
            }
        }

        private static IGen<T> ResolveError(OverrideMemberErrorKind overrideMemberError) =>
            overrideMemberError switch
            {
                OverrideMemberErrorKind.InvalidExpression invalidExpression =>
                    Error(
                        $"expression '{invalidExpression.Expression}' was invalid, an overriding expression may only contain member access"),
                OverrideMemberErrorKind.AttemptedOverrideOnRegisteredGen
                    attemptedOverrideOnRegisteredGen =>
                    Error(
                        $"attempted to override expression '{attemptedOverrideOnRegisteredGen.Expression}' on type '{typeof(T)}', but overriding members for registered types is not currently supported (GitHub issue: https://github.com/nth-commit/GalaxyCheck/issues/346)"),
                _ => throw new NotSupportedException($"Type not supported in switch: {overrideMemberError.GetType()}")
            };

        private static IGen<T> Error(string message) => Gen.Advanced.Error<T>(nameof(ReflectedGen<T>), message);
    }
}
