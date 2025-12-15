using TmSimulator.Core.Machine;

namespace TmSimulator.Core.Simulation;

public class TraceEntry
{
    public int Step { get; init; }
    public string State { get; init; } = string.Empty;
    public char Read { get; init; }
    public char Write { get; init; }
    public Direction Move { get; init; }
    public long HeadPosition { get; init; }
}
