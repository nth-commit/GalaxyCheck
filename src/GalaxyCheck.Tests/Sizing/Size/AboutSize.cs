using System;
using Xunit;
using S = GalaxyCheck.Internal.Sizing;

namespace Tests.Sizing.Size
{
    public class AboutSize
    {
        [Fact]
        public void MinSizeIsZero()
        {
            Assert.Equal(0, S.Size.MinValue.Value);
        }

        [Fact]
        public void MaxSizeIs100()
        {
            Assert.Equal(100, S.Size.MaxValue.Value);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-10)]
        public void WhenCreatedWithNegativeSize_ItThrows(int value)
        {
            Action test = () =>
            {
                new S.Size(value);
            };

            var exception = Assert.Throws<ArgumentOutOfRangeException>(test);
            Assert.Equal("'value' must be between 0 and 100 (Parameter 'value')", exception.Message);
        }

        [Theory]
        [InlineData(101)]
        [InlineData(1000)]
        public void WhenCreatedWithSizeGreaterThan100_ItThrows(int value)
        {
            Action test = () =>
            {
                new S.Size(value);
            };

            var exception = Assert.Throws<ArgumentOutOfRangeException>(test);
            Assert.Equal("'value' must be between 0 and 100 (Parameter 'value')", exception.Message);
        }
    }
}
