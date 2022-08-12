using FluentAssertions;
using GalaxyCheck;
using GalaxyCheck.Runners.Replaying;
using NebulaCheck;
using System;
using System.Collections.Generic;
using System.Linq;
using Gen = NebulaCheck.Gen;
using Property = NebulaCheck.Property;
using Test = NebulaCheck.Test;

namespace Tests.V2.RunnerTests.CheckTests
{
    public class AboutReplay
    {
        [Property]
        public NebulaCheck.IGen<Test> IfTheReplayIsInAnInvalidFormat_ItThrows() =>
            from gen in DomainGen.Gen()
            from func in Gen.Function<object, bool>(Gen.Boolean())
            from replay in Gen.Constant("0")
            select Property.ForThese(() =>
            {
                var property = gen.ForAll(func);

                Action test = () => property.Check(replay: replay);

                test.Should()
                    .Throw<GalaxyCheck.Exceptions.GenErrorException>()
                    .WithMessage("Error decoding replay string:*");
            });

        [Property]
        public NebulaCheck.IGen<Test> IfTheReplayEncodesAnInvalidShrinkPath_ItThrows() =>
            from func in Gen.Function(Gen.Boolean())
            from replaySeed in Gen.Int32()
            from replaySize in Gen.Int32().Between(0, 100)
            from replayPath in Gen.Int32().ListOf().WithCountGreaterThanEqual(1)
            select Property.ForThese(() =>
            {
                var property = GalaxyCheck.Property.Nullary(func);
                var replay = ReplayEncoding.Encode(CreateReplay(replaySeed, replaySize, replayPath));

                Action test = () => property.Check(replay: replay);

                test.Should()
                    .Throw<GalaxyCheck.Exceptions.GenErrorException>()
                    .WithMessage("Error replaying last example, given replay string was no longer valid.*");
            });

        [Property]
        public NebulaCheck.IGen<Test> IfTheReplayEncodesAGenError_ItThrows() =>
            from func in Gen.Function<object, bool>(Gen.Boolean())
            from replaySeed in Gen.Int32()
            from replaySize in Gen.Int32().Between(0, 100)
            from replayPath in Gen.Int32().ListOf()
            select Property.ForThese(() =>
            {
                var property = GalaxyCheck.Gen.Advanced.Error<object>("", "").ForAll(func);
                var replay = ReplayEncoding.Encode(CreateReplay(replaySeed, replaySize, replayPath));

                Action test = () => property.Check(replay: replay);

                test.Should()
                    .Throw<GalaxyCheck.Exceptions.GenErrorException>()
                    .WithMessage("Error replaying last example, given replay string was no longer valid.*");
            });

        [Property]
        public NebulaCheck.IGen<Test> ItOnlyCallsThePropertyFunctionOnce() =>
            from gen in DomainGen.Gen()
            from func in Gen.Function<object, bool>(Gen.Boolean())
            from seed in Gen.Int32()
            let property0 = gen.ForAll(func)
            let result0 = property0.Check(seed: seed)
            where result0.Falsified
            select Property.ForThese(() =>
            {
                var numberOfCalls = 0;
                var property1 = gen.ForAll(x =>
                {
                    numberOfCalls++;
                    return func(x);
                });

                var result1 = property1.Check(replay: result0.Counterexample!.Replay);

                numberOfCalls.Should().Be(1);
            });

        private static Replay CreateReplay(int replaySeed, int replaySize, IEnumerable<int> replayPath)
        {
            return new Replay(
                GalaxyCheck.Gens.Parameters.GenParameters.Parse(replaySeed, replaySize),
                replayPath);
        }
    }
}
