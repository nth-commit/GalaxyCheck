using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using System;
using System.Linq;
using Test = NebulaCheck.Test;
using Property = NebulaCheck.Property;
using Gen = NebulaCheck.Gen;

namespace Tests.V2.GenTests.DateTimeGenTests
{
    public class AboutValidation
    {
        [Property]
        public NebulaCheck.IGen<Test> ItErrorsWhenFromIsAfterUntil() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            from fromDateTime in Gen.DateTime().From(DateTime.MinValue.AddMilliseconds(1))
            from untilDateTime in Gen.DateTime().Until(fromDateTime.AddMilliseconds(-1))
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen.DateTime().From(fromDateTime).Until(untilDateTime);

                Action action = () => gen.SampleOne(seed: seed, size: size);

                action.Should()
                    .Throw<GalaxyCheck.Exceptions.GenErrorException>()
                    .WithGenErrorMessage("'from' datetime cannot be after 'until' datetime");
            });
    }
}
