using TmSimulator.Core.Machine;

namespace TmSimulator.Core.Simulation;

public class StepResult
{
    public int StepIndex { get; init; }
    public string StateBefore { get; init; } = string.Empty;
    public string StateAfter { get; init; } = string.Empty;
    public char ReadSymbol { get; init; }
    public char WriteSymbol { get; init; }
    public Direction Move { get; init; }
    public long HeadPosition { get; init; }
    public SimulationStatus Status { get; init; }
    public string Message { get; init; } = string.Empty;
    public TransitionRule? Rule { get; init; }
}
