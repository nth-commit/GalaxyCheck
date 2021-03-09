using FluentAssertions;
using GalaxyCheck.Runners;
using NebulaCheck;
using NebulaCheck.Xunit;
using Snapshooter;
using Snapshooter.Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Tests.V2.RendererTests
{
    public class AboutUnaryExamples
    {
        [Property]
        public IGen<Test> ItRendersOneLine() =>
            from value in DomainGen.Choose(DomainGen.Any(), DomainGen.AnyList())
            select Property.ForThese(() =>
            {
                var rendering = ExampleRenderer.Render(new ExampleViewModel.Unary(value));

                rendering.Should().ContainSingle();
            });

        public static TheoryData<object?, string> Values => new TheoryData<object?, string>
        {
            { null, "null" },
            { 1, "1" },
            { new List<int> { 1, 2, 3 }, "[1,2,3]" },
            { new List<string> { "a", "b", "c" }, @"[""a"",""b"",""c""]" },
            { new RecordObj(1, 2, 3), @"{""A"":1,""B"":2,""C"":3}" },
            { new ClassObj(1, 2, 3), @"{""A"":1,""B"":2,""C"":3}" },
            { new { a = 1, b = 2, c = 3 }, @"{""a"":1,""b"":2,""c"":3}" },
            { new Func<int, bool>((_) => true), "Func`2[Int32,Boolean]" },
            { new Func<Func<int, bool>>(() => (_) => true), "Func`1[Func`2[Int32,Boolean]]" },
            { new Action(() => { }), "Action" },
            { new Tuple<int, int, int>(1, 2, 3), @"{""Item1"":1,""Item2"":2,""Item3"":3}" },
            { (1, 2, 3), @"{""Item1"":1,""Item2"":2,""Item3"":3}" },
            { (1, new List<int> { 1, 2, 3 }, new Func<int, bool>((_) => true)), @"{""Item1"":1,""Item2"":[1,2,3],""Item3"":""Delegate""/*Func`2[Int32,Boolean]*/}" }
        };

        [Theory]
        [MemberData(nameof(Values))]
        public void Examples(object value, string expectedRendering)
        {
            var rendering = ExampleRenderer.Render(new ExampleViewModel.Unary(value)).Single();

            rendering.Should().Be(expectedRendering);
        }

        public record RecordObj(int A, int B, int C);

        public class ClassObj
        {
            public ClassObj(int a, int b, int c)
            {
                A = a;
                B = b;
                C = c;
            }

            public int A { get; }
            public int B { get; }
            public int C { get; }
        }
    }
}
