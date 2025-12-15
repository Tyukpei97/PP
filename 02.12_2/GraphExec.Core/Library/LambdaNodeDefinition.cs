using System;
using System.Collections.Generic;
using GraphExec.Core.Graph;
using GraphExec.Core.Types;
using GraphExec.Core.Values;

namespace GraphExec.Core.Library;

internal sealed class LambdaNodeDefinition : NodeDefinition
{
    private readonly Func<NodeExecutionContext, IReadOnlyList<GraphValue?>, EvaluationOutcome> _impl;

    public LambdaNodeDefinition(string code, string displayName, string category, IReadOnlyList<PortDefinition> inputs, GraphType outputType, Func<NodeExecutionContext, IReadOnlyList<GraphValue?>, EvaluationOutcome> impl, string? description = null)
        : base(code, displayName, category, inputs, new List<PortDefinition> { new("out", outputType) }, description)
    {
        _impl = impl;
    }

    public override EvaluationOutcome Evaluate(NodeExecutionContext context, IReadOnlyList<GraphValue?> inputs)
    {
        return _impl(context, inputs);
    }
}
