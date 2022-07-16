using System.Threading.Tasks;
using GalaxyCheck.ExampleSpaces;

internal delegate ExplorationOutcome AnalyzeExploration<in T>(IExample<T> example);

internal delegate ValueTask<ExplorationOutcome> AnalyzeExplorationAsync<in T>(IExample<T> example);
