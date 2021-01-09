using GalaxyCheck.Abstractions;

namespace GalaxyCheck.Gens
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
            ISize InitialSize,
            IRng NextRng,
            ISize NextSize);

        private readonly BaseGenIterationParameters _parameters;

        private GenIterationBuilder(BaseGenIterationParameters parameters)
        {
            _parameters = parameters;
        }

        public GenInstance<T> ToInstance<T>(IExampleSpace<T> exampleSpace) => new GenInstance<T>(
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
