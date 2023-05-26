using GalaxyCheck.Configuration;
using Snapshooter;
using Snapshooter.Xunit;
using System.IO;
using System.Threading.Tasks;

namespace IntegrationSample
{
    internal class ConfigureGalaxyCheck : IConfigureGlobal
    {
        public void Configure(IGlobalConfiguration instance)
        {
            instance.GenSnapshots.AssertSnapshotMatches = (snapshot) =>
            {
                var snapshotName = $"{snapshot.TestClassName}.{snapshot.TestMethodName}_{snapshot.Seed}.snap";
                var directoryName = Path.GetDirectoryName(snapshot.TestFileName)!;
                Snapshot.Match(snapshot.Output, new SnapshotFullName(snapshotName, directoryName));
                return Task.CompletedTask;
            };
        }
    }
}
