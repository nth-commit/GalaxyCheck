using FluentAssertions;
using GalaxyCheck.Runners;
using NebulaCheck;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Xunit;

namespace Tests.V2.RendererTests
{
    public class AboutUnaryExamples
    {
        [Property]
        public IGen<Test> ItRendersOneLine() =>
            from value in Gen.Choose(DomainGen.Any(), DomainGen.AnyList())
            select Property.ForThese(() =>
            {
                var rendering = ExampleRenderer.Render(new object[] { value });

                rendering.Should().ContainSingle();
            });

        [Property]
        public IGen<Test> ItRendersDateUsingTheCurrentCulture() =>
            from value in Gen.DateTime()
            select Property.ForThese(() =>
            {
                var rendering = ExampleRenderer.Render(new object[] { value }).Single();

                rendering.Should().Be(value.ToString(CultureInfo.CurrentCulture));
            });

        [Fact]
        public void ItCanHandleCircularReferences()
        {
            // Xunit theories cannot, apparently - so this needs to be its own Fact.
            var obj = new CircularlyReferencingRecord();

            var rendering = ExampleRenderer.Render(new object[] { obj }).Single();

            rendering.Should().Be("{ Self = <Circular reference> }");
        }

        public static TheoryData<object?, string> Values => new TheoryData<object?, string>
        {
            { null, "null" },
            { 1, "1" },
            { 1.5m, "1.5" },
            { "hiiiiooo", "hiiiiooo" },
            { new List<int> { 1, 2, 3 }, "[1, 2, 3]" },
            { new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, "[1, 2, 3, 4, 5, 6, 7, 8, 9, 10, ...]" },
            { new Dictionary<string, int> { { "A", 1 }, { "B", 2 } }, "[{ Key = A, Value = 1 }, { Key = B, Value = 2 }]" },
            { new Dictionary<RecordObj, int> { { new RecordObj(1, 2, 3), 4 } }, "[{ Key = { A = 1, B = 2, C = 3 }, Value = 4 }]" },
            { new RecordObj(1, 2, 3), "{ A = 1, B = 2, C = 3 }" },
            { new RecordObjWithList(new List<int> { 1, 2, 3 }), "{ As = [1, 2, 3] }" },
            { new RecordObjWithManyProperties(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11), "{ A = 1, B = 2, C = 3, D = 4, E = 5, F = 6, G = 7, H = 8, I = 9, J = 10, ... }" },
            { new { a = new { b = new { c = new { d = new { e = new { f = new { g = new { h = new { i = new { j = new { k = new { } } } } } } } } } } } }, "{ a = { b = { c = { d = { e = { f = { g = { h = { i = { j = ... } } } } } } } } } }" },
            { new ClassObj(1, 2, 3), "{ A = 1, B = 2, C = 3 }" },
            { new { a = 1, b = 2, c = 3 }, "{ a = 1, b = 2, c = 3 }" },
            { new Func<int, bool>((_) => true), "System.Func`2[System.Int32,System.Boolean]" },
            { new Func<Func<int, bool>>(() => (_) => true), "System.Func`1[System.Func`2[System.Int32,System.Boolean]]" },
            { new Action(() => { }), "System.Action" },
            { new Tuple<int, int, int>(1, 2, 3), "(Item1 = 1, Item2 = 2, Item3 = 3)" },
            { (1, 2, 3), "(Item1 = 1, Item2 = 2, Item3 = 3)" },
            { (1, 2, 3, 4, 5, 6, 7, 8), "(Item1 = 1, Item2 = 2, Item3 = 3, Item4 = 4, Item5 = 5, Item6 = 6, Item7 = 7, Rest = ...)" },
            { Operations.Create, "Create" },
            { new FaultyRecord(), "<Rendering failed>" },
        };

        [Theory]
        [MemberData(nameof(Values))]
        public void Examples(object value, string expectedRendering)
        {
            var rendering = ExampleRenderer.Render(new object[] { value }).Single();

            rendering.Should().Be(expectedRendering);
        }

        public record RecordObj(int A, int B, int C);

        public record RecordObjWithList(List<int> As);

        public record RecordObjWithManyProperties(int A, int B, int C, int D, int E, int F, int G, int H, int I, int J, int K);

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

        public record CircularlyReferencingRecord()
        {
            public CircularlyReferencingRecord Self => this;
        }

        public record FaultyRecord()
        {
            public int A => throw new Exception("Waaaaam waaaaaaaam");
        }

        public enum Operations
        {
            Create,
            Read
        }
    }
}
