using System;
using System.Collections.Generic;
using System.Linq;

namespace GraphExec.Core.Types;

public enum GraphTypeKind
{
    Int,
    Double,
    String,
    Bool,
    List,
    Tuple,
    Function
}

/// <summary>
/// Простая типовая модель для узлов графа.
/// </summary>
public sealed class GraphType : IEquatable<GraphType>
{
    public GraphTypeKind Kind { get; }
    public GraphType? ElementType { get; }
    public IReadOnlyList<GraphType> Arguments { get; } = Array.Empty<GraphType>();
    public GraphType? ReturnType { get; }

    private GraphType(GraphTypeKind kind, GraphType? elementType = null, IReadOnlyList<GraphType>? args = null, GraphType? returnType = null)
    {
        Kind = kind;
        ElementType = elementType;
        Arguments = args ?? Array.Empty<GraphType>();
        ReturnType = returnType;
    }

    public static GraphType Int { get; } = new(GraphTypeKind.Int);
    public static GraphType Double { get; } = new(GraphTypeKind.Double);
    public static GraphType String { get; } = new(GraphTypeKind.String);
    public static GraphType Bool { get; } = new(GraphTypeKind.Bool);
    public static GraphType TupleOf(GraphType a, GraphType b) => new(GraphTypeKind.Tuple, args: new[] { a, b });
    public static GraphType ListOf(GraphType element) => new(GraphTypeKind.List, elementType: element);
    public static GraphType FunctionOf(IEnumerable<GraphType> args, GraphType returnType)
        => new(GraphTypeKind.Function, args: args.ToArray(), returnType: returnType);

    public string DisplayName => Kind switch
    {
        GraphTypeKind.Int => "Целое",
        GraphTypeKind.Double => "Вещественное",
        GraphTypeKind.String => "Строка",
        GraphTypeKind.Bool => "Логическое",
        GraphTypeKind.List => $"Список<{ElementType?.DisplayName}>",
        GraphTypeKind.Tuple => $"Кортеж<{string.Join(",", Arguments.Select(a => a.DisplayName))}>",
        GraphTypeKind.Function => $"Функция({string.Join(", ", Arguments.Select(a => a.DisplayName))}) → {ReturnType?.DisplayName}",
        _ => Kind.ToString()
    };

    public bool IsAssignableFrom(GraphType other)
    {
        if (ReferenceEquals(this, other) || Equals(other))
            return true;

        // Неявное расширение Int -> Double допускается
        if (Kind == GraphTypeKind.Double && other.Kind == GraphTypeKind.Int)
            return true;

        if (Kind != other.Kind)
            return false;

        return Kind switch
        {
            GraphTypeKind.List => ElementType!.IsAssignableFrom(other.ElementType!),
            GraphTypeKind.Tuple => Arguments.Count == other.Arguments.Count &&
                                   Arguments.Zip(other.Arguments, (a, b) => a.IsAssignableFrom(b)).All(x => x),
            GraphTypeKind.Function => Arguments.Count == other.Arguments.Count &&
                                      Arguments.Zip(other.Arguments, (a, b) => a.IsAssignableFrom(b)).All(x => x) &&
                                      ReturnType!.IsAssignableFrom(other.ReturnType!),
            _ => true
        };
    }

    public override string ToString() => DisplayName;

    public bool Equals(GraphType? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (Kind != other.Kind) return false;
        return Kind switch
        {
            GraphTypeKind.List => Equals(ElementType, other.ElementType),
            GraphTypeKind.Tuple => Arguments.SequenceEqual(other.Arguments),
            GraphTypeKind.Function => Arguments.SequenceEqual(other.Arguments) && Equals(ReturnType, other.ReturnType),
            _ => true
        };
    }

    public override bool Equals(object? obj) => Equals(obj as GraphType);

    public override int GetHashCode()
    {
        var hash = (int)Kind * 397;
        if (ElementType != null) hash ^= ElementType.GetHashCode();
        foreach (var a in Arguments)
            hash = HashCode.Combine(hash, a.GetHashCode());
        if (ReturnType != null) hash = HashCode.Combine(hash, ReturnType.GetHashCode());
        return hash;
    }
}
