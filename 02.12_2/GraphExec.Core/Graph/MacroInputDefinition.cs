using System.Collections.Generic;
using GraphExec.Core.Types;
using GraphExec.Core.Values;

namespace GraphExec.Core.Graph;

/// <summary>
/// Внутренний узел для подстановки значений в макрос.
/// </summary>
public sealed class MacroInputDefinition : NodeDefinition
{
    public MacroInputDefinition(GraphType type, string name)
        : base($"macro.input.{name}", $"Вход {name}", "Макросы", new List<PortDefinition>(), new List<PortDefinition> { new("out", type) })
    {
    }

    public override EvaluationOutcome Evaluate(NodeExecutionContext context, IReadOnlyList<GraphValue?> inputs)
    {
        if (context.TryGetPresetValue(out var value) && value != null)
            return EvaluationOutcome.Single(value, "out");

        return EvaluationOutcome.Error("Значение входа макроса не передано");
    }
}
