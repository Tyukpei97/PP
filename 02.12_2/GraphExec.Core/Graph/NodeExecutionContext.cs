namespace GraphExec.Core.Graph;

public sealed class NodeExecutionContext
{
    public GraphState Graph { get; }
    public GraphEvaluator Evaluator { get; }
    public NodeInstance Node { get; }
    private readonly IReadOnlyDictionary<string, Values.GraphValue>? _preset;

    public NodeExecutionContext(GraphState graph, GraphEvaluator evaluator, NodeInstance node, IReadOnlyDictionary<string, Values.GraphValue>? preset = null)
    {
        Graph = graph;
        Evaluator = evaluator;
        Node = node;
        _preset = preset;
    }

    public bool TryGetPresetValue(out Values.GraphValue? value)
    {
        if (_preset != null && _preset.TryGetValue(Node.Id, out var v))
        {
            value = v;
            return true;
        }

        value = null;
        return false;
    }
}
