using System;
using System.Collections.Generic;
using System.Linq;
using GraphExec.Core.Values;

namespace GraphExec.Core.Graph;

public sealed class GraphEvaluation
{
    public GraphState Graph { get; }
    public IReadOnlyDictionary<string, EvaluationOutcome> Results { get; }
    public IReadOnlyList<string> GlobalErrors { get; }

    public GraphEvaluation(GraphState graph, IReadOnlyDictionary<string, EvaluationOutcome> results, IReadOnlyList<string>? errors = null)
    {
        Graph = graph;
        Results = results;
        GlobalErrors = errors ?? Array.Empty<string>();
    }

    public EvaluationOutcome? TryGet(string nodeId) => Results.TryGetValue(nodeId, out var v) ? v : null;

    public GraphValue? TryGetValue(string nodeId, string port)
    {
        if (Results.TryGetValue(nodeId, out var res) && res.Outputs.TryGetValue(port, out var val))
            return val;
        return null;
    }
}

public sealed class GraphEvaluator
{
    private readonly GraphValidator _validator = new();
    private readonly Dictionary<string, Dictionary<string, EvaluationOutcome>> _cache = new();

    public GraphEvaluation Evaluate(GraphState graph, IReadOnlyDictionary<string, GraphValue>? preset = null, IEnumerable<string>? changedNodes = null)
    {
        var errors = _validator.Validate(graph).ToList();
        if (errors.Any())
            return new GraphEvaluation(graph, new Dictionary<string, EvaluationOutcome>(), errors);

        var order = _validator.TopologicalSort(graph);
        var results = new Dictionary<string, EvaluationOutcome>();
        var affected = changedNodes != null ? CollectAffected(graph, changedNodes) : null;

        foreach (var node in order)
        {
            var inputs = BuildInputs(graph, node, results);
            var signature = BuildSignature(inputs);

            var skipCache = affected != null && affected.Contains(node.Id);
            if (!skipCache && _cache.TryGetValue(node.Id, out var bySig) && bySig.TryGetValue(signature, out var cached))
            {
                results[node.Id] = cached;
                continue;
            }

            if (preset != null && preset.TryGetValue(node.Id, out var presetValue))
            {
                var portName = node.Definition.Outputs.First().Name;
                var outcome = EvaluationOutcome.Single(presetValue, portName);
                StoreCache(node.Id, signature, outcome);
                results[node.Id] = outcome;
                continue;
            }

            var ctx = new NodeExecutionContext(graph, this, node, preset);
            var nodeOutcome = node.Definition.Evaluate(ctx, inputs);
            StoreCache(node.Id, signature, nodeOutcome);
            results[node.Id] = nodeOutcome;
        }

        return new GraphEvaluation(graph, results);
    }

    private IReadOnlyList<GraphValue?> BuildInputs(GraphState graph, NodeInstance node, Dictionary<string, EvaluationOutcome> results)
    {
        var list = new List<GraphValue?>();
        foreach (var port in node.Definition.Inputs)
        {
            var incoming = graph.Edges.FirstOrDefault(e => e.ToNode == node.Id && e.ToPort == port.Name);
            if (incoming == null)
            {
                list.Add(null);
                continue;
            }

            if (results.TryGetValue(incoming.FromNode, out var sourceOutcome) &&
                sourceOutcome.Outputs.TryGetValue(incoming.FromPort, out var v))
            {
                list.Add(v);
            }
            else
            {
                list.Add(null);
            }
        }
        return list;
    }

    private string BuildSignature(IReadOnlyList<GraphValue?> inputs)
    {
        return string.Join("|", inputs.Select(i => i?.GetHashCode().ToString() ?? "null"));
    }

    private void StoreCache(string nodeId, string signature, EvaluationOutcome outcome)
    {
        if (!_cache.TryGetValue(nodeId, out var bySig))
        {
            bySig = new Dictionary<string, EvaluationOutcome>();
            _cache[nodeId] = bySig;
        }

        bySig[signature] = outcome;
    }

    private HashSet<string> CollectAffected(GraphState graph, IEnumerable<string> changed)
    {
        var affected = new HashSet<string>(changed);
        var queue = new Queue<string>(changed);
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var edge in graph.OutgoingFrom(current))
            {
                if (affected.Add(edge.ToNode))
                    queue.Enqueue(edge.ToNode);
            }
        }

        return affected;
    }

    public GraphEvaluation EvaluateSubgraph(GraphState subGraph, IReadOnlyDictionary<string, GraphValue> preset)
        => Evaluate(subGraph, preset);
}
