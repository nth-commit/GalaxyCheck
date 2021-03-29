namespace GalaxyCheck
{
    using GalaxyCheck.Gens;

    public static partial class Gen
    {
        /// <summary>
        /// Generates strings of increasing complexity. By default, the generator producess strings of lengths 0 - 20,
        /// using the full ASCII character set (including control characters). Aspects of string generation can be
        /// refined using the builder methods on <see cref="IStringGen" />.
        /// </summary>
        /// <returns>The new generator.</returns>
        public static IStringGen String() => new StringGen();
    }

    public static partial class Extensions
    {
        /// <summary>
        /// Constrains the generator so that it only produces strings with lengths within the supplied range
        /// (inclusive).
        /// </summary>
        /// <param name="x">The first bound of the range.</param>
        /// <param name="y">The second bound of the range.</param>
        /// <returns>A new generator with the constraint applied.</returns>
        public static IStringGen WithLengthBetween(this IStringGen gen, int x, int y)
        {
            var minLength = x > y ? y : x;
            var maxLength = x > y ? x : y;
            return gen.WithLengthGreaterThanEqual(minLength).WithLengthLessThanEqual(maxLength);
        }

        /// <summary>
        /// Constrains the generator so that it only produces strings with greater than the given Length.
        /// </summary>
        /// <param name="exclusiveMinLength">The minimum length that generated strings should be constrained to.</param>
        /// <returns>A new generator with the constraint applied.</returns>
        public static IStringGen WithLengthGreaterThan(this IStringGen gen, int exclusiveMinLength) =>
            gen.WithLengthGreaterThanEqual(exclusiveMinLength + 1);

        /// <summary>
        /// Constrains the generator so that it only produces strings less than the given Length.
        /// </summary>
        /// <param name="exclusiveMaxLength">The maximum length that generated strings should be constrained to.</param>
        /// <returns>A new generator with the constraint applied.</returns>
        public static IStringGen WithLengthLessThan(this IStringGen gen, int exclusiveMaxLength) =>
            gen.WithLengthLessThanEqual(exclusiveMaxLength - 1);
    }
}

namespace GalaxyCheck.Gens
{
    using GalaxyCheck.Gens.Internal;
    using GalaxyCheck.Gens.Iterations.Generic;
    using GalaxyCheck.Gens.Parameters;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public interface IStringGen : IGen<string>
    {
        /// <summary>
        /// Constrains the generator so that it only produces strings from a particular type of character.
        /// </summary>
        /// <param name="charType">The character type to constrain the strings to.
        /// <see cref="Gen.Char(Gen.CharType)" /> is a flags enumeration, so constraining to multiple character types
        /// can be acheived with bitwise operators.</param>
        /// <returns>A new generator with the constraint applied.</returns>
        IStringGen FromCharacters(Gen.CharType charType);

        /// <summary>
        /// Constrains the generator so that it only produces strings from the given characters.
        /// </summary>
        /// <param name="chars">The collection of characters which can be used to generate strings.</param>
        /// <returns>A new generator with the constraint applied.</returns>
        IStringGen FromCharacters(IEnumerable<char> chars);

        /// <summary>
        /// Constrains the generator so that it only produces strings of the given length.
        /// </summary>
        /// <param name="length">The length to constrain the strings to.</param>
        /// <returns>A new generator with the constraint applied.</returns>
        IStringGen WithLength(int length);

        /// <summary>
        /// Constrains the generator so that it only produces strings of at least the given length.
        /// </summary>
        /// <param name="minLength">The minimum length to constrain the strings to.</param>
        /// <returns>A new generator with the constraint applied.</returns>
        IStringGen WithLengthGreaterThanEqual(int minLength);

        /// <summary>
        /// Constrains the generator so that it only produces strings of at most the given length.
        /// </summary>
        /// <param name="maxLength">The maximum length to constrain the strings to.</param>
        /// <returns>A new generator with the constraint applied.</returns>
        IStringGen WithLengthLessThanEqual(int maxLength);

        /// <summary>
        /// Modifies how the generator biases string lengths. Specifically, <see cref="Gen.Bias.WithSize" /> will edge
        /// the generator towards produces the strings with the maximum length, as the size parameter increases. 
        /// <see cref="Gen.Bias.None" /> will create strings with uniformly distributed lengths, irrespective of size.
        /// </summary>
        /// <param name="lengthBias">The bias to modify the generator by.</param>
        /// <returns>A new generator with the biasing effect applied.</returns>
        IStringGen WithLengthBias(Gen.Bias lengthBias);
    }

    internal class StringGen : BaseGen<string>, IStringGen
    {
        private abstract record StringGenCharConfig
        {
            private StringGenCharConfig()
            {
            }

            public record FromCharType(Gen.CharType CharType) : StringGenCharConfig;

            public record FromEnumerable(IEnumerable<char> Chars) : StringGenCharConfig;
        }

        private abstract record StringGenLengthConfig
        {
            private StringGenLengthConfig()
            {
            }

            public record Specific(int Length) : StringGenLengthConfig;

            public record Ranged(int? MinLength, int? MaxLength) : StringGenLengthConfig;
        }

        private record StringGenConfig(
            StringGenCharConfig? CharConfig,
            StringGenLengthConfig? LengthConfig,
            Gen.Bias? LengthBias);

        private readonly StringGenConfig _config;

        private StringGen(StringGenConfig config)
        {
            _config = config;
        }

        public StringGen()
            : this(new StringGenConfig(CharConfig: null, LengthConfig: null, LengthBias: null))
        {
        }

        public IStringGen FromCharacters(Gen.CharType charType) =>
            WithPartialConfig(charConfig: new StringGenCharConfig.FromCharType(charType));

        public IStringGen FromCharacters(IEnumerable<char> chars) =>
            WithPartialConfig(charConfig: new StringGenCharConfig.FromEnumerable(chars));

        public IStringGen WithLength(int length) =>
            WithPartialConfig(lengthConfig: new StringGenLengthConfig.Specific(length));

        public IStringGen WithLengthGreaterThanEqual(int minLength) =>
            WithPartialConfig(lengthConfig: new StringGenLengthConfig.Ranged(
                MinLength: minLength,
                MaxLength: _config.LengthConfig is StringGenLengthConfig.Ranged rangedLengthConfig
                    ? rangedLengthConfig.MaxLength
                    : null));

        public IStringGen WithLengthLessThanEqual(int maxLength) =>
            WithPartialConfig(lengthConfig: new StringGenLengthConfig.Ranged(
                MinLength: _config.LengthConfig is StringGenLengthConfig.Ranged rangedLengthConfig
                    ? rangedLengthConfig.MinLength
                    : null,
                MaxLength: maxLength));

        public IStringGen WithLengthBias(Gen.Bias lengthBias) =>
            WithPartialConfig(lengthBias: lengthBias);

        private IStringGen WithPartialConfig(
            StringGenCharConfig? charConfig = null,
            StringGenLengthConfig? lengthConfig = null,
            Gen.Bias? lengthBias = null)
        {
            var newConfig = new StringGenConfig(
                charConfig ?? _config.CharConfig,
                lengthConfig ?? _config.LengthConfig,
                lengthBias ?? _config.LengthBias);

            return new StringGen(newConfig);
        }

        protected override IEnumerable<IGenIteration<string>> Run(GenParameters parameters) =>
            CreateGen(_config).Advanced.Run(parameters);

        private static IGen<string> CreateGen(StringGenConfig config)
        {
            var charGen = CreateCharGen(config.CharConfig);
            var listGen = CreateListGen(charGen, config.LengthConfig, config.LengthBias);
            return listGen.Select(chars => new string(chars.ToArray()));
        }

        private static IGen<char> CreateCharGen(StringGenCharConfig? charConfig)
        {
            static IGen<char> Error(string message) => Gen.Advanced.Error<char>(nameof(StringGen), message);

            charConfig ??= new StringGenCharConfig.FromCharType(Gen.CharType.All);

            if (charConfig is StringGenCharConfig.FromCharType charTypeConfig)
            {
                var charType = charTypeConfig.CharType;
                if (charType <= 0 || charType > Gen.CharType.All)
                {
                    return Error("'charType' was not a valid flag value");
                }

                return Gen.Char(charType);
            }
            else if (charConfig is StringGenCharConfig.FromEnumerable enumerableConfig)
            {
                var charList = enumerableConfig.Chars.ToList();

                if (charList.Any() == false)
                {
                    return Error("'chars' must not be empty");
                }

                return Gen.Element(charList);
            }
            else
            {
                throw new Exception("Fatal: Unhandled case");
            }
        }

        private static IGen<IReadOnlyList<char>> CreateListGen(IGen<char> charGen, StringGenLengthConfig? lengthConfig, Gen.Bias? lengthBias)
        {
            static IGen<IReadOnlyList<char>> Error(string message) =>
                Gen.Advanced.Error<IReadOnlyList<char>>(nameof(StringGen), message);

            lengthConfig ??= new StringGenLengthConfig.Ranged(null, null);

            if (lengthConfig is StringGenLengthConfig.Specific specificLengthConfig)
            {
                if (specificLengthConfig.Length < 0)
                {
                    return Error("'length' cannot be negative");
                }

                return charGen.ListOf().WithCount(specificLengthConfig.Length);
            }
            else if (lengthConfig is StringGenLengthConfig.Ranged rangedLengthConfig)
            {
                var minLength = rangedLengthConfig.MinLength ?? 0;
                var maxLength = rangedLengthConfig.MaxLength ?? 100;

                if (minLength < 0)
                {
                    return Error("'minLength' cannot be negative");
                }

                if (maxLength < 0)
                {
                    return Error("'maxLength' cannot be negative");
                }

                if (minLength > maxLength)
                {
                    return Error("'minLength' cannot be greater than 'maxLength'");
                }

                return lengthBias == null
                    ? charGen.ListOf().WithCountBetween(minLength, maxLength)
                    : charGen.ListOf().WithCountBetween(minLength, maxLength).WithCountBias(lengthBias.Value);
            }
            else
            {
                throw new Exception("Fatal: Unhandled case");
            }
        }
    }   
}