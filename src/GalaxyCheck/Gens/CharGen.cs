

namespace GalaxyCheck
{
    using GalaxyCheck.Gens;

    public static partial class Gen
    {
        /// <summary>
        /// Generates characters of increasing complexity. Initially generates whitespace only characters, then
        /// includes alphabetic characters, then numeric characters, and so on (until it eventually generates control
        /// characters). The character sets generated from can be restricted using the <see cref="Char(CharType)"/>
        /// flag.
        /// </summary>
        /// <example>
        /// <code>
        /// var alphaNumericGen = Gen.Char(Gen.CharType.Alphabetic | Gen.CharType.Numeric);
        /// </code>
        /// </example>
        /// <param name="charType"></param>
        /// <returns>A new generator of characters.</returns>
        public static IGen<char> Char(CharType charType = CharType.All) => new CharGen(charType);
    }
}

namespace GalaxyCheck.Gens
{
    using GalaxyCheck.Gens.Internal;
    using GalaxyCheck.Gens.Iterations.Generic;
    using GalaxyCheck.Gens.Parameters;
    using System.Collections.Generic;
    using System.Linq;

    internal class CharGen : BaseGen<char>, IGen<char>
    {
        private readonly Gen.CharType _charType;

        public CharGen(Gen.CharType charType)
        {
            _charType = charType;
        }

        protected override IEnumerable<IGenIteration<char>> Run(GenParameters parameters) =>
            CreateGen(_charType).Advanced.Run(parameters);

        private static IGen<char> CreateGen(Gen.CharType charType)
        {
            if (charType <= 0 || charType > Gen.CharType.All)
            {
                return Gen.Advanced.Error<char>(nameof(CharGen), "'charType' was not a valid flag value");
            }

            var chars = CharacterSetsByFlag
                .Aggregate(
                    Enumerable.Empty<char>(),
                    (acc, curr) => charType.HasFlag(curr.Key) ? acc.Concat(curr.Value) : acc)
                .ToList();

            return Gen.Int32().Between(0, chars.Count - 1).Select(i => chars[i]);
        }

        private static readonly IReadOnlyDictionary<Gen.CharType, IReadOnlyList<char>> CharacterSetsByFlag =
            new Dictionary<Gen.CharType, IReadOnlyList<char>>
            {
                { Gen.CharType.Whitespace, new List<char> { ' ' } },
                { Gen.CharType.Alphabetical, CreateCharRanges((97, 122), (65, 90)) },
                { Gen.CharType.Numeric, CreateCharRanges((48, 57)) },
                { Gen.CharType.Symbol, CreateCharRanges((33, 47), (58, 64), (91, 96), (123, 126)) },
                { Gen.CharType.Extended, CreateCharRanges((128, 255)) },
                { Gen.CharType.Control, CreateCharRanges((0, 31), (127, 127)) },
            };

        private static IReadOnlyList<char> CreateCharRanges(params (int start, int end)[] ranges) => ranges
            .Aggregate(
                Enumerable.Empty<int>(),
                (acc, curr) => acc.Concat(Enumerable.Range(curr.start, curr.end - curr.start + 1)))
            .Select(x => (char)x)
            .ToList();
    }
}
