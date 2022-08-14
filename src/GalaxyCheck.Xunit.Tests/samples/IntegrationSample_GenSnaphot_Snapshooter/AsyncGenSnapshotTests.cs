using GalaxyCheck;
using System.Threading.Tasks;

namespace IntegrationSample
{
    public class AsyncGenSnapshotTests
    {
        public record GeneratedInput(int Integer, int Integer2, int Integer3);

        [GenSnapshot(Iterations = 1)]
        public async Task<object> TaskOutput(object input)
        {
            await Task.Delay(1);
            return input;
        }

        [GenSnapshot(Iterations = 1)]
        public async ValueTask<object> ValueTaskOuput(object input)
        {
            await Task.Delay(1);
            return input;
        }
    }
}
