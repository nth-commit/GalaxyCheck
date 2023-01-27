using System;
using FluentAssertions;
using GalaxyCheck;

namespace Tests.V2.GenTests.CreateGenTests
{
    public class AboutValidation
    {
        [NebulaCheck.Property]
        public NebulaCheck.Property ItWrapsExceptionsAsGenErrors() =>
            NebulaCheck.Property.ForAll(DomainGen.Seed(), DomainGen.Size(), (seed, size) =>
            {
                // Arrange
                var gen = Gen.Create<int>(_ => throw new Exception("oops"));

                // Act
                Action action = () => gen.SampleOne(seed, size);

                // Assert
                action
                    .Should()
                    .Throw<Exceptions.GenErrorException>()
                    .WithGenErrorMessage(@"Exception thrown.

System.Exception: oops*");
            });
    }
}
