using System;
using System.Collections.Generic;
using System.Linq;
using GraphExec.Core.Types;

namespace GraphExec.Core.Values;

public sealed class GraphValue : IEquatable<GraphValue>
{
    public GraphType Type { get; }
    public object? Data { get; }

    private GraphValue(GraphType type, object? data)
    {
        Type = type;
        Data = data;
    }

    public static GraphValue FromInt(int v) => new(GraphType.Int, v);
    public static GraphValue FromDouble(double v) => new(GraphType.Double, v);
    public static GraphValue FromBool(bool v) => new(GraphType.Bool, v);
    public static GraphValue FromString(string v) => new(GraphType.String, v);
    public static GraphValue FromList(GraphType elementType, IReadOnlyList<GraphValue> elements)
        => new(GraphType.ListOf(elementType), elements);
    public static GraphValue FromFunction(FunctionValue function) => new(GraphType.FunctionOf(function.ArgumentTypes, function.ReturnType), function);

    public static GraphValue FromTuple(GraphValue a, GraphValue b)
        => new(GraphType.TupleOf(a.Type, b.Type), new[] { a, b });

    public T Unwrap<T>() => Data is T typed ? typed : throw new InvalidCastException($"Значение имеет тип {Data?.GetType()}");

    public override string ToString()
    {
        if (Type.Kind == GraphTypeKind.Function && Data is FunctionValue f)
            return f.Display;

        return Type.Kind switch
        {
            GraphTypeKind.Int => Data?.ToString() ?? "—",
            GraphTypeKind.Double => Data?.ToString() ?? "—",
            GraphTypeKind.Bool => (Data is bool b) ? (b ? "Истина" : "Ложь") : "—",
            GraphTypeKind.String => $"\"{Data}\"",
            GraphTypeKind.List when Data is IReadOnlyList<GraphValue> list => $"[{string.Join(", ", list.Select(x => x.ToString()))}]",
            GraphTypeKind.Tuple when Data is IReadOnlyList<GraphValue> tuple => $"({string.Join(", ", tuple.Select(x => x.ToString()))})",
            _ => Data?.ToString() ?? "—"
        };
    }

    public bool Equals(GraphValue? other)
    {
        if (other is null) return false;
        if (!Type.Equals(other.Type)) return false;
        if (ReferenceEquals(this, other)) return true;
        if (Type.Kind == GraphTypeKind.List)
        {
            var a = Data as IReadOnlyList<GraphValue>;
            var b = other.Data as IReadOnlyList<GraphValue>;
            if (a == null || b == null || a.Count != b.Count) return false;
            return a.Zip(b, (x, y) => x.Equals(y)).All(v => v);
        }

        return Equals(Data, other.Data);
    }

    public override bool Equals(object? obj) => Equals(obj as GraphValue);

    public override int GetHashCode()
    {
        var hash = Type.GetHashCode();
        if (Data is IReadOnlyList<GraphValue> list)
        {
            foreach (var v in list)
                hash = HashCode.Combine(hash, v.GetHashCode());
        }
        else
        {
            hash = HashCode.Combine(hash, Data?.GetHashCode() ?? 0);
        }
        return hash;
    }
}
