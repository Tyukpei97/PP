using TmSimulator.Core.Simulation;

namespace TmSimulator.Core.Analysis;

public class ComparisonResult
{
    public string Input { get; set; } = string.Empty;
    public SimulationStatus StatusA { get; set; }
    public SimulationStatus StatusB { get; set; }
    public string OutputA { get; set; } = string.Empty;
    public string OutputB { get; set; } = string.Empty;
    public string Verdict { get; set; } = string.Empty;
}
