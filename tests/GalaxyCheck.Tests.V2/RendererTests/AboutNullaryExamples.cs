﻿using FluentAssertions;
using GalaxyCheck.Runners;
using Xunit;

namespace Tests.V2.RendererTests
{
    public class AboutNullaryExamples
    {
        [Fact]
        public void ItRendersOneLineIndicatingNoValue()
        {
            var rendering = ExampleRenderer.Render(new ExampleViewModel.Nullary());

            rendering.Should().ContainSingle().Subject.Should().Be("(no value)");
        }
    }
}
