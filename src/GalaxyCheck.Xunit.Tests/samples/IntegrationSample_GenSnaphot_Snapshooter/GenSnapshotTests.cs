using GalaxyCheck;

namespace IntegrationSample
{
    public class GenSnapshotTests
    {
        public record GeneratedInput(int Integer, int Integer2, int Integer3);

        [GenSnapshot(Iterations = 1)]
        public GeneratedInput TypedOutput(GeneratedInput input) => input;

        [GenSnapshot(Iterations = 1)]
        public object ObjectOutput(object input) => input;
    }
}
