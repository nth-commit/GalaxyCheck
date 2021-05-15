namespace GalaxyCheck
{
    using GalaxyCheck.Gens;
    using System.Linq;

    public static partial class Gen
    {
        /// <summary>
        /// Creates a generator which chooses between values of the source generators at a probability defined by their
        /// given weights.
        /// </summary>
        /// <param name="weightedGens">The source generators to choose between, with their respective weights. If all
        /// source generators have the same weight, then they are equiprobable.</param>
        /// <returns>A new generator sourced from the given generators.</returns>
        public static IGen<T> Choose<T>(params (IGen<T> gen, int weight)[] weightedGens)
        {
            IChooseGen<T> initial = new ChooseGen<T>();
            return weightedGens.Aggregate(initial, (acc, curr) => acc.WithChoice(curr.gen, curr.weight));
        }

        /// <summary>
        /// Creates a generator which chooses between values of the source generators at an equal probability.
        /// </summary>
        /// <param name="gens">The source generators to choose from.</param>
        /// <returns>A new generator sourced from the given generators.</returns>
        public static IGen<T> Choose<T>(params IGen<T>[] gens) => Choose(gens.Select(g => (g, 1)).ToArray());

        /// <summary>
        /// Creates an empty generator of choices. The generator should eventually be defined using the builder
        /// methods available on <see cref="IChooseGen{T}"/>.
        /// </summary>
        /// <returns>A new generator, which should eventually be sourced other generators.</returns>
        public static IChooseGen<T> Choose<T>() => new ChooseGen<T>();
    }

    public static partial class Extensions
    {
        /// <summary>
        /// Adds a source generator as a new choice for this <see cref="IChooseGen{T}"/>. All generators added using
        /// this method are assigned the same weight, making this is a convenient way to build an choosing generator
        /// from equiprobably source generators.
        /// </summary>
        /// <param name="gen">The generator to add the choice to.</param>
        /// <param name="alternative">The source generator to add as an choice.</param>
        /// <returns>A new choosing generator with the choice added.</returns>
        public static IChooseGen<T> WithChoice<T>(this IChooseGen<T> gen, IGen<T> alternative) =>
            gen.WithChoice(alternative, 1);
    }
}

namespace GalaxyCheck.Gens
{
    using GalaxyCheck.ExampleSpaces;
    using GalaxyCheck.Gens.Internal;
    using GalaxyCheck.Gens.Iterations.Generic;
    using GalaxyCheck.Gens.Parameters;
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    public interface IChooseGen<T> : IGen<T>
    {
        /// <summary>
        /// Adds a source generator as a new alternative for this <see cref="IChooseGen{T}"/>, with a probablity
        /// defined by the given weight.
        /// </summary>
        /// <param name="gen">The source generator to add as an alternative.</param>
        /// <param name="weight">The weight that the source generator should be selected. If the weights of all source
        /// generators are the same, then they are equiprobable.</param>
        /// <returns>A new alternative generator with the alternative added.</returns>
        IChooseGen<T> WithChoice(IGen<T> gen, int weight);
    }

    internal class ChooseGen<T> : BaseGen<T>, IChooseGen<T>
    {
        private record Choice(IGen<T> Gen, int Weight);

        private readonly ImmutableList<Choice> _choices;

        private ChooseGen(ImmutableList<Choice> choices)
        {
            _choices = choices;
        }

        public ChooseGen() : this(ImmutableList.Create<Choice>()) { }

        public IChooseGen<T> WithChoice(IGen<T> gen, int weight) =>
            new ChooseGen<T>(_choices.Add(new Choice(gen, weight)));

        protected override IEnumerable<IGenIteration<T>> Run(GenParameters parameters) =>
            CreateGenerator(_choices).Advanced.Run(parameters);

        private static IGen<T> CreateGenerator(ImmutableList<Choice> choices)
        {
            if (choices.Any() == false)
            {
                return Gen.Advanced.Error<T>(nameof(ChooseGen<T>), "'choices' must be non-empty");
            }

            if (choices.Any(choice => choice.Weight < 0))
            {
                return Gen.Advanced.Error<T>(nameof(ChooseGen<T>), "'choices' must not contain a negatively weighted generator");
            }

            if (choices.Sum(choice => choice.Weight) == 0)
            {
                return Gen.Advanced.Error<T>(nameof(ChooseGen<T>), "'choices' must contain at least one generator with a weight greater than zero");
            }

            var weightedIndices = new WeightedList<int>(choices.Select((c, i) => new WeightedElement<int>(c.Weight, i)));

            return GenIndexWeighted(weightedIndices)
                .Select(LookupUnweightedIndex(weightedIndices))
                .Unfold(unweightedIndex => UnfoldUnweightedIndex(choices.Count, unweightedIndex))
                .SelectMany(unweightedIndex => choices[unweightedIndex].Gen);
        }

        private static IGen<int> GenIndexWeighted(IReadOnlyList<int> weightedIndices)
        {
            return Gen
                .Int32()
                .Between(0, weightedIndices.Count - 1)
                .WithBias(Gen.Bias.None)
                .NoShrink();
        }

        private static Func<int, int> LookupUnweightedIndex(IReadOnlyList<int> weightedIndices) =>
            (weightedIndex) => weightedIndices[weightedIndex];

        private static IExampleSpace<int> UnfoldUnweightedIndex(int unweightedCount, int unweightedIndex) =>
            ExampleSpaceFactory.Int32(unweightedIndex, 0, 0, unweightedCount - 1);
    }
}
