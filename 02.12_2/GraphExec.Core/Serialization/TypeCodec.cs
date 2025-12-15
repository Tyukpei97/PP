using System;
using System.Collections.Generic;
using System.Linq;
using GraphExec.Core.Types;

namespace GraphExec.Core.Serialization;

public sealed class TypeDto
{
    public string Kind { get; set; } = string.Empty;
    public TypeDto? Element { get; set; }
    public List<TypeDto>? Args { get; set; }
    public TypeDto? Return { get; set; }
}

public static class TypeCodec
{
    public static TypeDto ToDto(GraphType type)
    {
        return new TypeDto
        {
            Kind = type.Kind.ToString(),
            Element = type.ElementType != null ? ToDto(type.ElementType) : null,
            Args = type.Arguments.Any() ? type.Arguments.Select(ToDto).ToList() : null,
            Return = type.ReturnType != null ? ToDto(type.ReturnType) : null
        };
    }

    public static GraphType FromDto(TypeDto dto)
    {
        var kind = Enum.Parse<GraphTypeKind>(dto.Kind);
        return kind switch
        {
            GraphTypeKind.Int => GraphType.Int,
            GraphTypeKind.Double => GraphType.Double,
            GraphTypeKind.String => GraphType.String,
            GraphTypeKind.Bool => GraphType.Bool,
            GraphTypeKind.List => GraphType.ListOf(FromDto(dto.Element!)),
            GraphTypeKind.Tuple => GraphType.TupleOf(FromDto(dto.Args![0]), FromDto(dto.Args![1])),
            GraphTypeKind.Function => GraphType.FunctionOf(dto.Args!.Select(FromDto).ToList(), FromDto(dto.Return!)),
            _ => GraphType.String
        };
    }
}
