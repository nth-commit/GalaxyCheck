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
///     - Plugin-able strategies for handling nullables, tuples, ienumerables etc.
///     - Fields
///     - Render ctor params in path more sensibly
///     - Use in parameter gen
/// </summary>
namespace GalaxyCheck
{
    public static partial class Gen
    {
        public static IAutoGenFactory AutoFactory() => new AutoGenFactory();

        public static IAutoGen<T> Auto<T>() => AutoFactory().Create<T>();
    }
}

namespace GalaxyCheck.Gens
{
    public interface IAutoGenFactory
    {
        IAutoGenFactory RegisterType<T>(IGen<T> gen);

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
        }

        public IAutoGenFactory RegisterType<T>(IGen<T> gen) =>
            new AutoGenFactory(_registeredGensByType.SetItem(typeof(T), gen));

        public IAutoGen<T> Create<T>() => new AutoGen<T>(_registeredGensByType);
    }

    public interface IAutoGen<T> : IGen<T>
    {
        IAutoGen<T> OverrideMember<TMember>(
            Expression<Func<T, TMember>> memberSelector,
            IGen<TMember> memberGen);
    }

    internal record AutoGenMemberOverride(
        string Path,
        IGen Gen);

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
                return Gen.Advanced.Error<T>(
                    nameof(AutoGen<T>),
                    $"expression '{errorExpression}' was invalid, an overridding expression may only contain member access");
            }

            return AutoGenBuilder
                .Build(
                    typeof(T),
                    registeredGensByType,
                    memberOverrides,
                    message => Gen.Advanced.Error<T>(nameof(AutoGen<T>), message),
                    AutoGenHandlerContext.Create(typeof(T)))
                .Cast<T>();
        }
    }
}
