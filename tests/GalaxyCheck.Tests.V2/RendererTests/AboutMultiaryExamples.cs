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
            from values in DomainGen.AnyList()
            select Property.ForThese(() =>
            {
                var unaryRenderings = values
                    .Select(value => new ExampleViewModel.Unary(value))
                    .SelectMany((vm, index) => ExampleRenderer.Render(vm).Select(line => $"[{index}] = {line}"));

                var multiaryRendering = ExampleRenderer.Render(new ExampleViewModel.Multiary(values.ToArray()));

                multiaryRendering.Should().BeEquivalentTo(unaryRenderings);
            });
    }
}
