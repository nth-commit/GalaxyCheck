using GalaxyCheck.Gens;
using GalaxyCheck.Gens.AutoGenHelpers;
using GalaxyCheck.Gens.Internal;
using GalaxyCheck.Gens.Iterations.Generic;
using GalaxyCheck.Gens.Parameters;
using GalaxyCheck.Internal;
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
        public static IAutoGen<T> Auto<T>() => new AutoGen<T>();
    }
}

namespace GalaxyCheck.Gens
{
    public interface IAutoGen<T> : IGen<T>
    {
        IAutoGen<T> RegisterType<TRegister>(IGen<TRegister> gen);

        IAutoGen<T> OverrideMember<TMember>(
            Expression<Func<T, TMember>> memberSelector,
            IGen<TMember> memberGen);
    }

    internal record AutoGenMemberOverride(
        string Path,
        IGen Gen);

    internal class AutoGen<T> : BaseGen<T>, IAutoGen<T>
    {
        private static readonly ImmutableDictionary<Type, IGen> DefaultRegisteredGensByType =
            new Dictionary<Type, IGen>
            {
                { typeof(int), Gen.Int32() },
                { typeof(char), Gen.Char() },
                { typeof(string), Gen.String() }
            }.ToImmutableDictionary();

        private readonly ImmutableDictionary<Type, IGen> _registeredGensByType;
        private readonly ImmutableList<AutoGenMemberOverride> _memberOverrides;

        internal AutoGen(
            ImmutableDictionary<Type, IGen> registeredGensByType,
            ImmutableList<AutoGenMemberOverride> memberOverrides)
        {
            _registeredGensByType = registeredGensByType;
            _memberOverrides = memberOverrides;
        }

        public AutoGen() : this(
            DefaultRegisteredGensByType,
            ImmutableList.Create<AutoGenMemberOverride>())
        {
        }

        public IAutoGen<T> OverrideMember<TMember>(Expression<Func<T, TMember>> memberSelector, IGen<TMember> fieldGen)
        {
            var pathResult = PathResolver.FromExpression(memberSelector);
            return pathResult.Match<string, PathResolutionError, IAutoGen<T>>(
                fromLeft: path => new AutoGen<T>(
                    _registeredGensByType,
                    _memberOverrides.Add(new AutoGenMemberOverride(path, fieldGen))),
                fromRight: error => null!);
        }

        public IAutoGen<T> RegisterType<TRegister>(IGen<TRegister> gen) =>
            new AutoGen<T>(
                _registeredGensByType.SetItem(typeof(TRegister), gen),
                _memberOverrides);

        protected override IEnumerable<IGenIteration<T>> Run(GenParameters parameters) =>
            BuildGen(_registeredGensByType, _memberOverrides).Advanced.Run(parameters);

        private static IGen<T> BuildGen(
            IReadOnlyDictionary<Type, IGen> registeredGensByType,
            IReadOnlyList<AutoGenMemberOverride> memberOverrides) =>
                AutoGenBuilder
                    .Build(
                        typeof(T),
                        registeredGensByType,
                        memberOverrides,
                        message => Gen.Advanced.Error<T>(nameof(AutoGen<T>), message),
                        AutoGenFactoryContext.Create(typeof(T)))
                    .Cast<T>();
    }
}
