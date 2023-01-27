using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using System;
using System.Linq;
using Test = NebulaCheck.Test;
using Property = NebulaCheck.Property;
using Gen = NebulaCheck.Gen;
using GalaxyCheck.Gens;

namespace Tests.V2.GenTests.StringGenTests
{
    public class AboutValidation
    {
        [Property]
        public NebulaCheck.IGen<Test> ItErrorsWhenCharTypesIsNone() =>
            from charType in Gen
                .Choose(
                    Gen.Int32().LessThanEqual(0),
                    Gen.Int32().GreaterThan((int)GalaxyCheck.Gen.CharType.All))
                .Select(x => (GalaxyCheck.Gen.CharType)x)
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen
                    .String()
                    .FromCharacters(charType)
                    .WithLengthGreaterThanEqual(1);

                Action action = () => gen.SampleOne(seed: seed, size: size);

                action.Should()
                    .Throw<GalaxyCheck.Exceptions.GenErrorException>()
                    .WithGenErrorMessage("'charType' was not a valid flag value");
            });

        [Property]
        public NebulaCheck.IGen<Test> ItErrorsWhenCharsIsEmpty() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen
                    .String()
                    .FromCharacters(Enumerable.Empty<char>())
                    .WithLengthGreaterThanEqual(1);

                Action action = () => gen.SampleOne(seed: seed, size: size);

                action.Should()
                    .Throw<GalaxyCheck.Exceptions.GenErrorException>()
                    .WithGenErrorMessage("'chars' must not be empty");
            });

        [Property]
        public NebulaCheck.IGen<Test> ItErrorsWhenLengthIsNegative() =>
            from length in Gen.Int32().LessThan(0)
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen.String().WithLength(length);

                Action action = () => gen.SampleOne(seed: seed, size: size);

                action.Should()
                    .Throw<GalaxyCheck.Exceptions.GenErrorException>()
                    .WithGenErrorMessage("'length' cannot be negative");
            });

        [Property]
        public NebulaCheck.IGen<Test> ItErrorsWhenTheMinimumLengthIsNegative() =>
            from minLength in Gen.Int32().LessThan(0)
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen.String().WithLengthGreaterThanEqual(minLength);

                Action action = () => gen.SampleOne(seed: seed, size: size);

                action.Should()
                    .Throw<GalaxyCheck.Exceptions.GenErrorException>()
                    .WithGenErrorMessage("'minLength' cannot be negative");
            });

        [Property]
        public NebulaCheck.IGen<Test> ItErrorsWhenTheMaximumLengthIsNegative() =>
            from maxLength in Gen.Int32().LessThan(0)
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen.String().WithLengthLessThanEqual(maxLength);

                Action action = () => gen.SampleOne(seed: seed, size: size);

                action.Should()
                    .Throw<GalaxyCheck.Exceptions.GenErrorException>()
                    .WithGenErrorMessage("'maxLength' cannot be negative");
            });

        [Property]
        public NebulaCheck.IGen<Test> ItErrorsWhenMinimumLengthIsGreaterThanMaximumLength() =>
            from minLength in Gen.Int32().Between(1, 100)
            from maxLength in Gen.Int32().GreaterThanEqual(0).LessThan(minLength)
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                void ShouldError(IStringGen gen)
                {
                    Action action = () => gen.SampleOne(seed: seed, size: size);

                    action.Should()
                        .Throw<GalaxyCheck.Exceptions.GenErrorException>()
                        .WithGenErrorMessage("'minLength' cannot be greater than 'maxLength'");
                }

                ShouldError(GalaxyCheck.Gen.String().WithLengthGreaterThanEqual(minLength).WithLengthLessThanEqual(maxLength));
                ShouldError(GalaxyCheck.Gen.String().WithLengthLessThanEqual(maxLength).WithLengthGreaterThanEqual(minLength));
            });
    }
}
