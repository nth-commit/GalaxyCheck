namespace GalaxyCheck
{
    using System.Linq;

    public static partial class Gen
    {
        /// <summary>
        /// Generates globally unique identifiers (<see cref="System.Guid"/>). The generation is unbiased, and the
        /// generated guids do not shrink to simpler renditions of themselves (for example, shrinking towards
        /// <see cref="System.Guid.Empty"/>). This is because guids should be "globally unique", we should never
        /// generate the same guid twice. Generation is essentially the same as <see cref="System.Guid.NewGuid"/>,
        /// except that it is controlled by the seed, so it will be stable across repeats. If you require unique guids,
        /// even across repetitions, don't use a generator - use <see cref="System.Guid.NewGuid"/>.
        /// </summary>
        /// <returns>The new generator.</returns>
        public static IGen<System.Guid> Guid() =>
            from b in Byte().WithBias(Bias.None).ListOf().WithCount(16).NoShrink()
            select new System.Guid(b.ToArray());
    }
}
