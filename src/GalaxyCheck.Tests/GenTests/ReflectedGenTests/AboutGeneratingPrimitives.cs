using FluentAssertions;
using GalaxyCheck;
using System;
using System.Drawing;
using System.Reflection;
using Xunit;

namespace Tests.V2.GenTests.ReflectedGenTests
{
    public class AboutGeneratingPrimitives
    {
        [Theory]
        [InlineData(typeof(short))]
        [InlineData(typeof(ushort))]
        [InlineData(typeof(int))]
        [InlineData(typeof(uint))]
        [InlineData(typeof(long))]
        [InlineData(typeof(ulong))]
        [InlineData(typeof(float))]
        [InlineData(typeof(double))]
        [InlineData(typeof(decimal))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(char))]
        [InlineData(typeof(byte))]
        [InlineData(typeof(sbyte))]
        [InlineData(typeof(string))]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(DateTimeOffset))]
        [InlineData(typeof(TimeSpan))]
        [InlineData(typeof(Guid))]
        [InlineData(typeof(Uri))]
        [InlineData(typeof(Color))]
        public void ItCanGenerateType(Type type)
        {
            // Arrange
            var method = typeof(Gen).GetMethod(nameof(Gen.Create), types: new[] { typeof(NullabilityInfo) });
            var genericMethod = method!.MakeGenericMethod(type);
            var gen = (IGen)genericMethod.Invoke(null, new object?[] { null })!;

            // Act
            Action action = () => gen.Cast<object>().SampleNTraversals(10, 0, 100);

            // Assert
            action.Should().NotThrow();
        }
    }
}
