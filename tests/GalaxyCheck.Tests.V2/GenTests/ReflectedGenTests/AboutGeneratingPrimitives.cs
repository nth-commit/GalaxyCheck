using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using System;
using System.Collections.Generic;
using System.Linq;
using Property = NebulaCheck.Property;
using Test = NebulaCheck.Test;

namespace Tests.V2.GenTests.ReflectedGenTests
{
    public class AboutGeneratingPrimitives
    {
        [Property]
        public NebulaCheck.IGen<Test> ItCanGenerateInt16s() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                List<short> SampleTraversal(GalaxyCheck.IGen<short> gen) => gen.SampleOneTraversal(seed, size);

                var gen0 = GalaxyCheck.Gen.Create<short>();
                var gen1 = GalaxyCheck.Gen.Int16();

                var sample0 = SampleTraversal(gen0);
                var sample1 = SampleTraversal(gen1);

                sample0.Should().BeEquivalentTo(sample1);
            });

        [Property]
        public NebulaCheck.IGen<Test> ItCanGenerateUnsignedInt16s() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                List<ushort> SampleTraversal(GalaxyCheck.IGen<ushort> gen) => gen.SampleOneTraversal(seed, size);

                var gen0 = GalaxyCheck.Gen.Create<ushort>();
                var gen1 = GalaxyCheck.Gen.Int16().Select(x => (ushort)x);

                var sample0 = SampleTraversal(gen0);
                var sample1 = SampleTraversal(gen1);

                sample0.Should().BeEquivalentTo(sample1);
            });

        [Property]
        public NebulaCheck.IGen<Test> ItCanGenerateInt32s() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                List<int> SampleTraversal(GalaxyCheck.IGen<int> gen) => gen.SampleOneTraversal(seed, size);

                var gen0 = GalaxyCheck.Gen.Create<int>();
                var gen1 = GalaxyCheck.Gen.Int32();

                var sample0 = SampleTraversal(gen0);
                var sample1 = SampleTraversal(gen1);

                sample0.Should().BeEquivalentTo(sample1);
            });

        [Property]
        public NebulaCheck.IGen<Test> ItCanGenerateUnsignedInt32s() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                List<uint> SampleTraversal(GalaxyCheck.IGen<uint> gen) => gen.SampleOneTraversal(seed, size);

                var gen0 = GalaxyCheck.Gen.Create<uint>();
                var gen1 = GalaxyCheck.Gen.Int32().Select(x => (uint)x);

                var sample0 = SampleTraversal(gen0);
                var sample1 = SampleTraversal(gen1);

                sample0.Should().BeEquivalentTo(sample1);
            });

        [Property]
        public NebulaCheck.IGen<Test> ItCanGenerateInt64s() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                List<long> SampleTraversal(GalaxyCheck.IGen<long> gen) => gen.SampleOneTraversal(seed, size);

                var gen0 = GalaxyCheck.Gen.Create<long>();
                var gen1 = GalaxyCheck.Gen.Int64();

                var sample0 = SampleTraversal(gen0);
                var sample1 = SampleTraversal(gen1);

                sample0.Should().BeEquivalentTo(sample1);
            });

        [Property]
        public NebulaCheck.IGen<Test> ItCanGenerateUnsignedInt64s() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                List<ulong> SampleTraversal(GalaxyCheck.IGen<ulong> gen) => gen.SampleOneTraversal(seed, size);

                var gen0 = GalaxyCheck.Gen.Create<ulong>();
                var gen1 = GalaxyCheck.Gen.Int64().Select(x => (ulong)x);

                var sample0 = SampleTraversal(gen0);
                var sample1 = SampleTraversal(gen1);

                sample0.Should().BeEquivalentTo(sample1);
            });

        [Property]
        public NebulaCheck.IGen<Test> ItCanGenerateChars() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                List<char> SampleTraversal(GalaxyCheck.IGen<char> gen) => gen.SampleOneTraversal(seed, size);

                var gen0 = GalaxyCheck.Gen.Create<char>();
                var gen1 = GalaxyCheck.Gen.Char();

                var sample0 = SampleTraversal(gen0);
                var sample1 = SampleTraversal(gen1);

                sample0.Should().BeEquivalentTo(sample1);
            });

        [Property]
        public NebulaCheck.IGen<Test> ItCanGenerateStrings() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                List<string> SampleTraversal(GalaxyCheck.IGen<string> gen) => gen.SampleOneTraversal(seed, size);

                var gen0 = GalaxyCheck.Gen.Create<string>();
                var gen1 = GalaxyCheck.Gen.String();

                var sample0 = SampleTraversal(gen0);
                var sample1 = SampleTraversal(gen1);

                sample0.Should().BeEquivalentTo(sample1);
            });

        [Property]
        public NebulaCheck.IGen<Test> ItCanGenerateBytes() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                List<byte> SampleTraversal(GalaxyCheck.IGen<byte> gen) => gen.SampleOneTraversal(seed, size);

                var gen0 = GalaxyCheck.Gen.Create<byte>();
                var gen1 = GalaxyCheck.Gen.Byte();

                var sample0 = SampleTraversal(gen0);
                var sample1 = SampleTraversal(gen1);

                sample0.Should().BeEquivalentTo(sample1);
            });

        [Property]
        public NebulaCheck.IGen<Test> ItCanGenerateGuids() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                List<Guid> SampleTraversal(GalaxyCheck.IGen<Guid> gen) => gen.SampleOneTraversal(seed, size);

                var gen0 = GalaxyCheck.Gen.Create<Guid>();
                var gen1 = GalaxyCheck.Gen.Guid();

                var sample0 = SampleTraversal(gen0);
                var sample1 = SampleTraversal(gen1);

                sample0.Should().BeEquivalentTo(sample1);
            });

        [Property]
        public NebulaCheck.IGen<Test> ItCanGenerateDateTimes() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                List<DateTime> SampleTraversal(GalaxyCheck.IGen<DateTime> gen) => gen.SampleOneTraversal(seed, size);

                var gen0 = GalaxyCheck.Gen.Create<DateTime>();
                var gen1 = GalaxyCheck.Gen.DateTime();

                var sample0 = SampleTraversal(gen0);
                var sample1 = SampleTraversal(gen1);

                sample0.Should().BeEquivalentTo(sample1);
            });
    }
}
