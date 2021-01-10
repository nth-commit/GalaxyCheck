using GalaxyCheck.Internal.ExampleSpaces;
using GalaxyCheck.Internal.Sizing;

namespace GalaxyCheck.Internal.GenIterations
{
    public class GenIterationBuilder
    {
        public static GenIterationBuilder FromIteration<T>(GenIteration<T> iteration) =>
            new GenIterationBuilder(new BaseGenIterationParameters(
                iteration.InitialRng,
                iteration.InitialSize,
                iteration.NextRng,
                iteration.NextSize));

        private record BaseGenIterationParameters(
            IRng InitialRng,
            Size InitialSize,
            IRng NextRng,
            Size NextSize);

        private readonly BaseGenIterationParameters _parameters;

        private GenIterationBuilder(BaseGenIterationParameters parameters)
        {
            _parameters = parameters;
        }

        public GenIterationBuilder WithInitialRng(IRng rng) => new GenIterationBuilder(new BaseGenIterationParameters(
            rng,
            _parameters.InitialSize,
            _parameters.NextRng,
            _parameters.NextSize));

        public GenIterationBuilder WithInitialSize(Size size) => new GenIterationBuilder(new BaseGenIterationParameters(
            _parameters.InitialRng,
            size,
            _parameters.NextRng,
            _parameters.NextSize));

        public GenIterationBuilder WithNextSize(Size size) => new GenIterationBuilder(new BaseGenIterationParameters(
            _parameters.InitialRng,
            _parameters.InitialSize,
            _parameters.NextRng,
            size));

        public GenInstance<T> ToInstance<T>(ExampleSpace<T> exampleSpace) => new GenInstance<T>(
            _parameters.InitialRng,
            _parameters.InitialSize,
            _parameters.NextRng,
            _parameters.NextSize,
            exampleSpace);

        public GenError<T> ToError<T>(string genName, string message) => new GenError<T>(
            _parameters.InitialRng,
            _parameters.InitialSize,
            _parameters.NextRng,
            _parameters.NextSize,
            genName,
            message);

        public GenDiscard<T> ToDiscard<T>() => new GenDiscard<T>(
            _parameters.InitialRng,
            _parameters.InitialSize,
            _parameters.NextRng,
            _parameters.NextSize);
    }
}
