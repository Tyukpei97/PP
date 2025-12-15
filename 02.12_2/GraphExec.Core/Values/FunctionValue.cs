using System;
using System.Collections.Generic;
using System.Linq;
using GraphExec.Core.Types;

namespace GraphExec.Core.Values;

public sealed class FunctionValue
{
    private readonly IReadOnlyList<GraphType> _baseArguments;
    private readonly Func<IReadOnlyList<GraphValue>, EvaluationOutcome> _root;
    private readonly IReadOnlyList<GraphValue> _captured;

    public IReadOnlyList<GraphType> ArgumentTypes { get; }
    public GraphType ReturnType { get; }
    public string Display { get; }

    public FunctionValue(IReadOnlyList<GraphType> baseArguments, GraphType returnType, Func<IReadOnlyList<GraphValue>, EvaluationOutcome> root, IReadOnlyList<GraphValue>? captured = null)
    {
        _baseArguments = baseArguments;
        ReturnType = returnType;
        _root = root;
        _captured = captured ?? Array.Empty<GraphValue>();
        ArgumentTypes = baseArguments.Skip(_captured.Count).ToArray();
        Display = $"λ({string.Join(", ", ArgumentTypes.Select(a => a.DisplayName))}) → {ReturnType.DisplayName}";
    }

    public EvaluationOutcome Invoke(IReadOnlyList<GraphValue> args)
    {
        var merged = _captured.Concat(args).ToList();
        if (merged.Count < _baseArguments.Count)
        {
            var partial = new FunctionValue(_baseArguments, ReturnType, _root, merged);
            return EvaluationOutcome.Single(GraphValue.FromFunction(partial), displayHint: "Частичное применение");
        }

        if (merged.Count > _baseArguments.Count)
            return EvaluationOutcome.Error($"Ожидалось {_baseArguments.Count} аргументов, получено {merged.Count}");

        return _root(merged);
    }
}
