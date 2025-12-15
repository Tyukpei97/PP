namespace TmSimulator.Core.Simulation;

public enum SimulationStatus
{
    Running,
    HaltedAccepting,
    HaltedNoRule,
    LoopDetected,
    StepLimitReached
}
