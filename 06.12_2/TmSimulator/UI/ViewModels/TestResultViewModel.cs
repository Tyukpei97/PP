using TmSimulator.Core.Analysis;
using TmSimulator.Core.Simulation;

namespace TmSimulator.UI.ViewModels;

public class TestResultViewModel
{
    public string Input { get; init; } = string.Empty;
    public SimulationStatus Status { get; init; }
    public int Steps { get; init; }
    public string Output { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public static TestResultViewModel FromResult(MachineRunResult result) => new()
    {
        Input = result.Input,
        Status = result.Status,
        Steps = result.Steps,
        Output = result.OutputTapeSnippet,
        Message = result.Message
    };
}
