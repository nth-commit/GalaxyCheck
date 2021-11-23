using FluentAssertions;
using GalaxyCheck.Runners;
using NebulaCheck;
using System.Linq;

namespace Tests.V2.RendererTests
{
    public class AboutMultiaryExamples
    {
        [Property]
        public IGen<Test> ItRendersLikeUnary() =>
            from values in DomainGen.AnyList().WithCountGreaterThan(1)
            select Property.ForThese(() =>
            {
                var unaryRenderings = values.SelectMany((value, index) => ExampleRenderer
                    .Render(new object[] { value })
                    .Select(line => $"[{index}] = {line}"));

                var multiaryRendering = ExampleRenderer.Render(values.ToArray());

                multiaryRendering.Should().BeEquivalentTo(unaryRenderings);
            });
    }
}
