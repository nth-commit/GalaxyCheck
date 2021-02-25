using GalaxyCheck.Internal.ExampleSpaces;
using GalaxyCheck.Internal.Sizing;

namespace GalaxyCheck.Internal.GenIterations
{
    public class GenIterationBuilder
    {
        public static GenIterationBuilder FromIteration<T>(IGenIteration<T> iteration) =>
            new GenIterationBuilder(new BaseGenIterationParameters(
                iteration.RepeatParameters,
                iteration.NextParameters));

        private record BaseGenIterationParameters(
            GenParameters RepeatParameters,
            GenParameters NextParameters);

        private readonly BaseGenIterationParameters _parameters;

        private GenIterationBuilder(BaseGenIterationParameters parameters)
        {
            _parameters = parameters;
        }

        public GenIterationBuilder WithInitialRng(IRng rng) => new GenIterationBuilder(new BaseGenIterationParameters(
            new GenParameters(rng, _parameters.RepeatParameters.Size),
            _parameters.NextParameters));

        public GenIterationBuilder WithInitialSize(Size size) => new GenIterationBuilder(new BaseGenIterationParameters(
            new GenParameters(_parameters.RepeatParameters.Rng, size),
            _parameters.NextParameters));

        public GenIterationBuilder WithNextRng(IRng rng) => new GenIterationBuilder(new BaseGenIterationParameters(
            _parameters.RepeatParameters,
            new GenParameters(rng, _parameters.NextParameters.Size)));

        public GenIterationBuilder WithNextSize(Size size) => new GenIterationBuilder(new BaseGenIterationParameters(
            _parameters.RepeatParameters,
            new GenParameters(_parameters.NextParameters.Rng, size)));

        public GenInstance<T> ToInstance<T>(IExampleSpace<T> exampleSpace) => new GenInstance<T>(
            _parameters.RepeatParameters,
            _parameters.NextParameters,
            exampleSpace);

        public GenError<T> ToError<T>(string genName, string message) => new GenError<T>(
            _parameters.RepeatParameters,
            _parameters.NextParameters,
            genName,
            message);

        public GenDiscard<T> ToDiscard<T>() => new GenDiscard<T>(
            _parameters.RepeatParameters,
            _parameters.NextParameters);
    }
}
