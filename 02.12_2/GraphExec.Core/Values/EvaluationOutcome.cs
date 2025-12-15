using System.Collections.Generic;

namespace GraphExec.Core.Values;

public sealed class EvaluationOutcome
{
    public bool HasError => ErrorMessage != null;
    public string? ErrorMessage { get; }
    public IReadOnlyDictionary<string, GraphValue> Outputs { get; }
    public string? DisplayHint { get; }

    private EvaluationOutcome(string? error, IReadOnlyDictionary<string, GraphValue> outputs, string? displayHint)
    {
        ErrorMessage = error;
        Outputs = outputs;
        DisplayHint = displayHint;
    }

    public static EvaluationOutcome Single(GraphValue value, string portName = "result", string? displayHint = null)
        => new(null, new Dictionary<string, GraphValue> { { portName, value } }, displayHint);

    public static EvaluationOutcome Many(IReadOnlyDictionary<string, GraphValue> outputs, string? displayHint = null)
        => new(null, outputs, displayHint);

    public static EvaluationOutcome Error(string message)
        => new(message, new Dictionary<string, GraphValue>(), null);
}
