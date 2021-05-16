using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using System;
using System.Linq;
using Test = NebulaCheck.Test;
using Property = NebulaCheck.Property;

namespace Tests.V2.GenTests.DateTimeGenTests
{
    public class AboutValidation
    {
        [Property]
        public NebulaCheck.IGen<Test> ItErrorsWhenFromIsAfterUntil() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            from fromDateTime in DomainGen.DateTime(minDateTime: DateTime.MinValue.AddMilliseconds(1))
            from untilDateTime in DomainGen.DateTime(maxDateTime: fromDateTime.AddMilliseconds(-1))
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen.DateTime().From(fromDateTime).Until(untilDateTime);

                Action action = () => gen.SampleOne(seed: seed, size: size);

                action.Should()
                    .Throw<GalaxyCheck.Exceptions.GenErrorException>()
                    .WithMessage("Error while running generator DateTimeGen: 'from' datetime cannot be after 'until' datetime");
            });
    }
}
