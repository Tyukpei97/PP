using TmSimulator.Core.Analysis;
using TmSimulator.Core.Simulation;

namespace TmSimulator.UI.ViewModels;

public class ComparisonResultViewModel
{
    public string Input { get; init; } = string.Empty;
    public SimulationStatus StatusA { get; init; }
    public SimulationStatus StatusB { get; init; }
    public string OutputA { get; init; } = string.Empty;
    public string OutputB { get; init; } = string.Empty;
    public string Verdict { get; init; } = string.Empty;

    public static ComparisonResultViewModel FromResult(ComparisonResult result) => new()
    {
        Input = result.Input,
        StatusA = result.StatusA,
        StatusB = result.StatusB,
        OutputA = result.OutputA,
        OutputB = result.OutputB,
        Verdict = result.Verdict
    };
}
