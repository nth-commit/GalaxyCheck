using System;

namespace GalaxyCheck
{
    public static partial class Gen
    {
        /// <summary>
        /// A flag which indicates how values produced by generators should scale with respect to the size parameter.
        /// </summary>
        public enum Bias
        {
            /// <summary>
            /// Generated values should not be biased by the size parameter.
            /// </summary>
            None = 0,

            /// <summary>
            /// Generated values should scale exponentially with the size parameter.
            /// </summary>
            WithSize = 2
        }

        [Flags]
        public enum CharType
        {
            Whitespace = 1 << 0,

            Alphabetical = 1 << 1,

            Numeric = 1 << 2,

            Symbol = 1 << 3,

            Extended = 1 << 4,

            Control = 1 << 5,

            All = Whitespace | Alphabetical | Numeric | Symbol | Extended | Control
        }

        /// <summary>
        /// A container which contains generators for advanced usage.
        /// </summary>
        public static partial class Advanced
        {
        }
    }
}
