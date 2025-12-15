using System.Collections.Generic;
using GraphExec.Core.Values;

namespace GraphExec.Core.Graph;

public abstract class NodeDefinition
{
    public string Code { get; }
    public string DisplayName { get; }
    public string Category { get; }
    public IReadOnlyList<PortDefinition> Inputs { get; }
    public IReadOnlyList<PortDefinition> Outputs { get; }
    public string? Description { get; }

    protected NodeDefinition(string code, string displayName, string category, IReadOnlyList<PortDefinition> inputs, IReadOnlyList<PortDefinition> outputs, string? description = null)
    {
        Code = code;
        DisplayName = displayName;
        Category = category;
        Inputs = inputs;
        Outputs = outputs;
        Description = description;
    }

    public abstract EvaluationOutcome Evaluate(NodeExecutionContext context, IReadOnlyList<GraphValue?> inputs);
}
