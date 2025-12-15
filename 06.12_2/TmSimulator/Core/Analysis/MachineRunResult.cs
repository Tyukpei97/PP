using TmSimulator.Core.Simulation;

namespace TmSimulator.Core.Analysis;

public class MachineRunResult
{
    public string Input { get; set; } = string.Empty;
    public SimulationStatus Status { get; set; }
    public int Steps { get; set; }
    public string OutputTapeSnippet { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
