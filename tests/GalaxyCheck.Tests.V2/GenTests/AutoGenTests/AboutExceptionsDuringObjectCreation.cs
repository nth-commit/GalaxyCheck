using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using System;
using System.Linq;
using Xunit;
using Property = NebulaCheck.Property;
using Test = NebulaCheck.Test;

namespace Tests.V2.GenTests.AutoGenTests
{
    public class AboutExceptionsDuringObjectCreation
    {
        private class ClassWithDefaultThrowingConstructor
        {
            public ClassWithDefaultThrowingConstructor()
            {
                throw new Exception("Test exception");
            }
        }

        [Property]
        public NebulaCheck.IGen<Test> ItErrorsWhenTheDefaultConstructorThrowsAnException() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen.Auto<ClassWithDefaultThrowingConstructor>();

                Action action = () => gen.SampleOne(seed: seed, size: size);

                action.Should()
                    .Throw<GalaxyCheck.Exceptions.GenErrorException>()
                    .WithMessage("Error while running generator AutoGen: 'System.Exception' was thrown while calling constructor with message 'Test exception'");
            });

        private class ClassWithNonDefaultThrowingConstructor
        {
            public ClassWithNonDefaultThrowingConstructor(int x)
            {
                throw new Exception("Test exception");
            }
        }

        [Property]
        public NebulaCheck.IGen<Test> ItErrorsWhenTheNonDefaultConstructorThrowsAnException() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen.Auto<ClassWithNonDefaultThrowingConstructor>();

                Action action = () => gen.SampleOne(seed: seed, size: size);

                action.Should()
                    .Throw<GalaxyCheck.Exceptions.GenErrorException>()
                    .WithMessage("Error while running generator AutoGen: 'System.Exception' was thrown while calling constructor with message 'Test exception'");
            });

        private class ClassWithThrowingPropertySetter
        {
            public int MyProperty {
                get
                {
                    return 0;
                }
                set
                {
                    throw new Exception("Test exception");
                }
            }

            public ClassWithThrowingPropertySetter()
            {
            }
        }

        [Property]
        public NebulaCheck.IGen<Test> ItErrorsWhenAPropertySetterThrowsAnException() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen.Auto<ClassWithThrowingPropertySetter>();

                Action action = () => gen.SampleOne(seed: seed, size: size);

                action.Should()
                    .Throw<GalaxyCheck.Exceptions.GenErrorException>()
                    .WithMessage("Error while running generator AutoGen: 'System.Exception' was thrown while setting property with message 'Test exception'");
            });
    }
}
