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

        public static TheoryData<object, string> Values => new TheoryData<object, string>
        {
            { 1, "Int32" },
            { new List<int> { 1, 2, 3 }, "ListInt32" },
            { new List<string> { "a", "b", "c" }, "ListString" },
            { new RecordObj(1, 2, 3), "RecordObject" },
            { new ClassObj(1, 2, 3), "ClassObject" },
            { new { a = 1, b = 2, c = 3 }, "AnonymousObject" },
            { new Func<int, bool>((_) => true), "FuncOfInt32ToBoolean" }
        };

        [Theory]
        [MemberData(nameof(Values))]
        public void Snapshots(object value, string description)
        {
            var rendering = ExampleRenderer.Render(new ExampleViewModel.Unary(value));

            Snapshot.Match(string.Join(Environment.NewLine, rendering), SnapshotNameExtension.Create(description));
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
