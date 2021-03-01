using Xunit;
using FsCheck;
using FsCheck.Xunit;
using GalaxyCheck;
using GC = GalaxyCheck;
using static Tests.TestUtils;

namespace Tests.Gen.GenericOperators
{
    [Properties(Arbitrary = new[] { typeof(ArbitraryGen) })]
    public class AboutCast
    {
        private class ReferenceType { }

        [Fact]
        public void ItCanUpcastReferenceTypes()
        {
            var reference = new ReferenceType();
            IGen gen = GC.Gen.Constant(reference);
            var genObj = GC.Gen.Constant<object>(reference);

            GenAssert.Equal(genObj, gen.Cast<object>(), 0);
        }

        [Fact]
        public void ItCanDowncastReferenceTypes()
        {
            var reference = new ReferenceType();
            var gen = GC.Gen.Constant(reference);
            IGen genObj = GC.Gen.Constant<object>(reference);

            GenAssert.Equal(gen, genObj.Cast<ReferenceType>(), 0);
        }

        [Fact]
        public void ItCanUpcastValueTypes()
        {
            IGen gen = GC.Gen.Int32();
            var genObj = GC.Gen.Int32().Select(x => (object)x);

            GenAssert.Equal(genObj, gen.Cast<object>(), 0);
        }

        [Fact]
        public void ItCanDowncastValueTypes()
        {
            var gen = GC.Gen.Int32();
            IGen genObj = GC.Gen.Int32().Select(x => (object)x);

            GenAssert.Equal(gen, genObj.Cast<int>(), 0);
        }
    }
}
