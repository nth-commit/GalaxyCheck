﻿using GalaxyCheck;
using GalaxyCheck.Gens;
using Moq;
using NebulaCheck;
using System.Linq;
using Gen = NebulaCheck.Gen;
using Property = NebulaCheck.Property;
using Test = NebulaCheck.Test;

namespace Tests.V2.GenTests.Int16GenTests
{
    public class AboutExtensions
    {
        [Property]
        public NebulaCheck.IGen<Test> BetweenDefersToUnderlyingMethods() =>
            from min in Gen.Int16()
            from max in Gen.Int16().GreaterThanEqual(min)
            select Property.ForThese(() =>
            {
                {
                    var mockGen = SetupMock();

                    mockGen.Object.Between(min, max);

                    mockGen.Verify(gen => gen.GreaterThanEqual(min), Times.Once);
                    mockGen.Verify(gen => gen.LessThanEqual(max), Times.Once);
                }

                {
                    var mockGen = SetupMock();

                    mockGen.Object.Between(max, min);

                    mockGen.Verify(gen => gen.GreaterThanEqual(min), Times.Once);
                    mockGen.Verify(gen => gen.LessThanEqual(max), Times.Once);
                }
            });

        [Property]
        public NebulaCheck.IGen<Test> GreaterThanDefersToUnderlyingMethod() =>
            from minExclusive in Gen.Int16()
            select Property.ForThese(() =>
            {
                var mockGen = SetupMock();

                mockGen.Object.GreaterThan(minExclusive);

                mockGen.Verify(gen => gen.GreaterThanEqual((short)(minExclusive + 1)), Times.Once);
            });

        [Property]
        public NebulaCheck.IGen<Test> LessThanDefersToUnderlyingMethod() =>
            from maxExclusive in Gen.Int16()
            select Property.ForThese(() =>
            {
                var mockGen = SetupMock();

                mockGen.Object.LessThan(maxExclusive);

                mockGen.Verify(gen => gen.LessThanEqual((short)(maxExclusive - 1)), Times.Once);
            });

        private static Mock<IIntGen<short>> SetupMock()
        {
            var mockGen = new Mock<IIntGen<short>>();
            mockGen.Setup((gen) => gen.GreaterThanEqual(It.IsAny<short>())).Returns(mockGen.Object);
            mockGen.Setup((gen) => gen.LessThanEqual(It.IsAny<short>())).Returns(mockGen.Object);
            return mockGen;
        }
    }
}
