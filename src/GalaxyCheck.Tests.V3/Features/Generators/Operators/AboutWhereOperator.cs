using GalaxyCheck;

namespace GalaxyCheck_Tests_V3.Features.Generators.Operators;

public class AboutWhereOperator
{
    [NebulaCheck.Property]
    public NebulaCheck.Property ItProducesValuesThatPassThePredicate()
    {
        return NebulaCheck.Property.ForAll(DomainGen.Seed(), DomainGen.Size(), Test);

        static void Test(int seed, int size)
        {
            // Arrange
            var testFunction = DummyTestFunctions.Int32.NonZero();
            var baseGen = Gen.Int32();
            var filteredGen = baseGen.Where(testFunction);

            // Act
            var sample = filteredGen.Sample(args => args with { Seed = seed, Size = size });

            // Assert
            sample.Should().OnlyContain(x => testFunction(x));
        }
    }

    [NebulaCheck.Property]
    public NebulaCheck.Property ItCanAdaptToPredicatesThatOnlyPassForLargerSizes()
    {
        return NebulaCheck.Property.ForAll(DomainGen.Seed(), DomainGen.Size(), Test);

        static void Test(int seed, int size)
        {
            // Arrange
            var testFunction = DummyTestFunctions.Size.Top50thPercentile();
            var baseGen = DummyGens.TheInputSize();
            var filteredGen = baseGen.Where(testFunction);

            // Act
            var act = () => filteredGen.Sample(args => args with { Seed = seed, Size = size });

            // Assert
            act.Should().NotThrow<Exceptions.GenExhaustionException>();
        }
    }
}
