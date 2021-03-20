using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using System.Linq;
using Gen = NebulaCheck.Gen;
using Property = NebulaCheck.Property;
using Test = NebulaCheck.Test;

namespace Tests.V2.GenTests.FunctionGenTests
{
    public class AboutValueProduction
    {
        public class NullaryFunction
        {
            [Property]
            public NebulaCheck.IGen<Test> ItIsPure() =>
                from returnGen in DomainGen.Gen()
                from seed in DomainGen.Seed()
                from size in DomainGen.Size()
                select Property.ForThese(() =>
                {
                    var gen = returnGen.FunctionOf();

                    var func = gen.SampleOne(seed: seed, size: size);

                    func().Should().Be(func());
                });
        }

        public class UnaryFunction
        {
            [Property]
            public NebulaCheck.IGen<Test> ItIsPure() =>
                from input in DomainGen.Any()
                from returnGen in DomainGen.Gen()
                from seed in DomainGen.Seed()
                from size in DomainGen.Size()
                select Property.ForThese(() =>
                {
                    var gen = returnGen.FunctionOf<object, object>();

                    var func = gen.SampleOne(seed: seed, size: size);

                    func(input).Should().Be(func(input));
                });

            [Property]
            public NebulaCheck.IGen<Test> ItIsAffectedByItsArgument() =>
                from variables in Gen.Int32().WithBias(Gen.Bias.None).Two().ListOf().OfCount(10)
                from seed in DomainGen.Seed()
                select Property.ForThese(() =>
                {
                    var gen = GalaxyCheck.Gen.Int32().FunctionOf<object, int>();

                    var func = gen.SampleOne(seed: seed, size: 100);

                    variables.Should().Contain(variable => func(variable.Item1) != func(variable.Item2));
                });
        }

        public class BinaryFunction
        {
            [Property]
            public NebulaCheck.IGen<Test> ItIsPure() =>
                from input in DomainGen.Any().Two()
                from returnGen in DomainGen.Gen()
                from seed in DomainGen.Seed()
                from size in DomainGen.Size()
                select Property.ForThese(() =>
                {
                    var gen = returnGen.FunctionOf<object, object, object>();

                    var func = gen.SampleOne(seed: seed, size: size);

                    func(input.Item1, input.Item2).Should().Be(func(input.Item1, input.Item2));
                });

            [Property]
            public NebulaCheck.IGen<Test> ItIsAffectedByItsArguments() =>
                from control in Gen.Int32()
                from variables in Gen.Int32().WithBias(Gen.Bias.None).Two().ListOf().OfCount(10)
                from seed in DomainGen.Seed()
                select Property.ForThese(() =>
                {
                    var gen = GalaxyCheck.Gen.Int32().FunctionOf<object, object, int>();

                    var func = gen.SampleOne(seed: seed, size: 100);

                    variables.Should().Contain(variable => func(variable.Item1, control) != func(variable.Item2, control));
                    variables.Should().Contain(variable => func(control, variable.Item1) != func(control, variable.Item2));
                });
        }

        public class TernaryFunction
        {
            [Property]
            public NebulaCheck.IGen<Test> ItIsPure() =>
            from input in DomainGen.Any().Three()
            from returnGen in DomainGen.Gen()
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var gen = returnGen.FunctionOf<object, object, object, object>();

                var func = gen.SampleOne(seed: seed, size: size);

                func(input.Item1, input.Item2, input.Item3).Should()
                    .Be(func(input.Item1, input.Item2, input.Item3));
            });

            [Property]
            public NebulaCheck.IGen<Test> ItIsAffectedByItsArguments() =>
                from controls in Gen.Int32().Two()
                from variables in Gen.Int32().WithBias(Gen.Bias.None).Two().ListOf().OfCount(10)
                from seed in DomainGen.Seed()
                select Property.ForThese(() =>
                {
                    var gen = GalaxyCheck.Gen.Int32().FunctionOf<object, object, object, int>();

                    var func = gen.SampleOne(seed: seed, size: 100);

                    variables.Should().Contain(variable =>
                        func(variable.Item1, controls.Item1, controls.Item2) !=
                        func(variable.Item2, controls.Item1, controls.Item2));
                    variables.Should().Contain(variable =>
                        func(controls.Item1, variable.Item1, controls.Item2) !=
                        func(controls.Item1, variable.Item2, controls.Item2));
                    variables.Should().Contain(variable =>
                        func(controls.Item1, controls.Item2, variable.Item1) !=
                        func(controls.Item1, controls.Item2, variable.Item2));
                });
        }

        public class QuarternaryFunction
        {
            [Property]
            public NebulaCheck.IGen<Test> ItIsPure() =>
                from input in DomainGen.Any().Four()
                from returnGen in DomainGen.Gen()
                from seed in DomainGen.Seed()
                from size in DomainGen.Size()
                select Property.ForThese(() =>
                {
                    var gen = returnGen.FunctionOf<object, object, object, object, object>();

                    var func = gen.SampleOne(seed: seed, size: size);

                    func(input.Item1, input.Item2, input.Item3, input.Item4).Should()
                        .Be(func(input.Item1, input.Item2, input.Item3, input.Item4));
                });

            [Property]
            public NebulaCheck.IGen<Test> ItIsAffectedByItsArguments() =>
                from controls in Gen.Int32().Three()
                from variables in Gen.Int32().WithBias(Gen.Bias.None).Two().ListOf().OfCount(10)
                from seed in DomainGen.Seed()
                select Property.ForThese(() =>
                {
                    var gen = GalaxyCheck.Gen.Int32().FunctionOf<object, object, object, object, int>();

                    var func = gen.SampleOne(seed: seed, size: 100);

                    variables.Should().Contain(variable =>
                        func(variable.Item1, controls.Item1, controls.Item2, controls.Item3) !=
                        func(variable.Item2, controls.Item1, controls.Item2, controls.Item3));
                    variables.Should().Contain(variable =>
                        func(controls.Item1, variable.Item1, controls.Item2, controls.Item3) !=
                        func(controls.Item1, variable.Item2, controls.Item2, controls.Item3));
                    variables.Should().Contain(variable =>
                        func(controls.Item1, controls.Item2, variable.Item1, controls.Item3) !=
                        func(controls.Item1, controls.Item2, variable.Item2, controls.Item3));
                    variables.Should().Contain(variable =>
                        func(controls.Item1, controls.Item2, controls.Item3, variable.Item1) !=
                        func(controls.Item1, controls.Item2, controls.Item3, variable.Item2));
                });
        }
    }
}
