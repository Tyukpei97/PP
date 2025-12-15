namespace TmSimulator.Core.Machine;

public record TransitionRule(
    string FromState,
    char ReadSymbol,
    string ToState,
    char WriteSymbol,
    Direction Move
);
