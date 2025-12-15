using System;
using System.Collections.Generic;
using System.Linq;
using GraphExec.Core.Types;
using GraphExec.Core.Values;

namespace GraphExec.Core.Graph;

public sealed record MacroInputBinding(string PortName, string PlaceholderNodeId, GraphType Type);
public sealed record MacroOutputBinding(string PortName, string SourceNodeId, string SourcePort);

/// <summary>
/// Макроузел, который инкапсулирует подграф.
/// </summary>
public sealed class MacroDefinition : NodeDefinition
{
    public GraphState SubGraph { get; }
    public IReadOnlyList<MacroInputBinding> InputBindings { get; }
    public IReadOnlyList<MacroOutputBinding> OutputBindings { get; }

    public MacroDefinition(string code, string displayName, GraphState subGraph, IReadOnlyList<MacroInputBinding> inputs, IReadOnlyList<MacroOutputBinding> outputs)
        : base(code, displayName, "Макросы", inputs.Select(i => new PortDefinition(i.PortName, i.Type)).ToList(), outputs.Select(o => new PortDefinition(o.PortName, subGraph.FindNode(o.SourceNodeId)?.Definition.Outputs.First(p => p.Name == o.SourcePort).Type ?? GraphType.String)).ToList())
    {
        SubGraph = subGraph;
        InputBindings = inputs;
        OutputBindings = outputs;
    }

    public override EvaluationOutcome Evaluate(NodeExecutionContext context, IReadOnlyList<GraphValue?> inputs)
    {
        if (inputs.Any(v => v is null))
            return BuildPartial(context, inputs);

        var preset = new Dictionary<string, GraphValue>();
        for (int i = 0; i < InputBindings.Count; i++)
            preset[InputBindings[i].PlaceholderNodeId] = inputs[i]!;

        var eval = context.Evaluator.EvaluateSubgraph(SubGraph, preset);
        if (eval.GlobalErrors.Any())
            return EvaluationOutcome.Error(string.Join(Environment.NewLine, eval.GlobalErrors));

        var outputs = new Dictionary<string, GraphValue>();
        foreach (var binding in OutputBindings)
        {
            var val = eval.TryGetValue(binding.SourceNodeId, binding.SourcePort);
            if (val == null)
                return EvaluationOutcome.Error($"Не удалось прочитать выход {binding.SourcePort}");
            outputs[binding.PortName] = val;
        }

        return outputs.Count == 1
            ? EvaluationOutcome.Single(outputs.Values.First(), OutputBindings.First().PortName)
            : EvaluationOutcome.Many(outputs);
    }

    private EvaluationOutcome BuildPartial(NodeExecutionContext context, IReadOnlyList<GraphValue?> inputs)
    {
        var baseArgs = InputBindings.Select(b => b.Type).ToList();
        var captured = new List<GraphValue>();
        for (int i = 0; i < inputs.Count; i++)
        {
            var v = inputs[i];
            if (v != null && captured.Count == i)
                captured.Add(v);
            else
                break;
        }
        var function = new FunctionValue(baseArgs, Outputs.First().Type, allArgs =>
        {
            var preset = new Dictionary<string, GraphValue>();
            for (int i = 0; i < InputBindings.Count; i++)
                preset[InputBindings[i].PlaceholderNodeId] = allArgs[i];

            var eval = context.Evaluator.EvaluateSubgraph(SubGraph, preset);
            if (eval.GlobalErrors.Any())
                return EvaluationOutcome.Error(string.Join(Environment.NewLine, eval.GlobalErrors));

            var outputs = new Dictionary<string, GraphValue>();
            foreach (var binding in OutputBindings)
            {
                var val = eval.TryGetValue(binding.SourceNodeId, binding.SourcePort);
                if (val == null)
                    return EvaluationOutcome.Error($"Не найден выход {binding.SourcePort} внутри макроса");
                outputs[binding.PortName] = val;
            }

            return outputs.Count == 1
                ? EvaluationOutcome.Single(outputs.Values.First(), OutputBindings.First().PortName)
                : EvaluationOutcome.Many(outputs);
        }, captured);

        return EvaluationOutcome.Single(GraphValue.FromFunction(function), displayHint: "Частичное применение макроса");
    }
}
