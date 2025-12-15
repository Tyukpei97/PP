using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GraphExec.Core.Graph;
using GraphExec.Core.Types;
using GraphExec.Core.Values;

namespace GraphExec.Core.Library;

/// <summary>
/// Каталог встроенных узлов.
/// </summary>
public static class NodeLibrary
{
    public static IReadOnlyList<NodeDefinition> All { get; }

    private static readonly Dictionary<string, NodeDefinition> _byCode;

    static NodeLibrary()
    {
        var items = new List<NodeDefinition>
        {
            Constant("const.int", "Целое", GraphType.Int),
            Constant("const.double", "Вещественное", GraphType.Double),
            Constant("const.string", "Строка", GraphType.String),
            Constant("const.bool", "Логическое", GraphType.Bool),
            Slider("source.slider", "Слайдер", GraphType.Double),
            TextInput("source.text", "Текстовый ввод"),
            ListLiteral(),

            BinaryInt("op.add", "Сложение", (a, b) => a + b),
            BinaryInt("op.sub", "Вычитание", (a, b) => a - b),
            BinaryInt("op.mul", "Умножение", (a, b) => a * b),
            Divide(),
            Not(),
            Length(),
            ToUpper(),
            Compose(),
            Curry(),
            Map(),
            Filter(),
            Sum(),
            GreaterThan()
        };

        All = items;
        _byCode = items.ToDictionary(x => x.Code, x => x);
    }

    public static NodeDefinition? Find(string code) => _byCode.TryGetValue(code, out var d) ? d : null;

    private static NodeDefinition Constant(string code, string caption, GraphType type)
    {
        return new LambdaNodeDefinition(code, caption, "Источники", Array.Empty<PortDefinition>(), type, (ctx, inputs) =>
        {
            var settings = ctx.Node.Settings;
            var hasValue = settings.TryGetValue("value", out var raw) ? raw : null;
            if (raw == null)
            {
                return EvaluationOutcome.Single(DefaultValue(type), "out", "По умолчанию");
            }

            var parsed = ParseValue(type, raw);
            return parsed == null
                ? EvaluationOutcome.Error("Некорректное значение источника")
                : EvaluationOutcome.Single(parsed, "out");
        }, $"Источник {caption}");
    }

    private static NodeDefinition Slider(string code, string caption, GraphType type)
    {
        return new LambdaNodeDefinition(code, caption, "Источники", Array.Empty<PortDefinition>(), type, (ctx, inputs) =>
        {
            var settings = ctx.Node.Settings;
            var valStr = settings.TryGetValue("value", out var raw) ? raw : "0";
            var parsed = double.TryParse(valStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var v) ? v : 0;
            var graphValue = type.Kind == GraphTypeKind.Int ? GraphValue.FromInt((int)Math.Round(parsed)) : GraphValue.FromDouble(parsed);
            return EvaluationOutcome.Single(graphValue, "out");
        }, "Числовой источник, связанный с ползунком");
    }

    private static NodeDefinition TextInput(string code, string caption)
    {
        return new LambdaNodeDefinition(code, caption, "Источники", Array.Empty<PortDefinition>(), GraphType.String, (ctx, inputs) =>
        {
            var raw = ctx.Node.Settings.TryGetValue("value", out var s) ? s : string.Empty;
            return EvaluationOutcome.Single(GraphValue.FromString(raw), "out");
        }, "Поле ввода текста");
    }

    private static NodeDefinition ListLiteral()
    {
        return new LambdaNodeDefinition("source.list", "Список", "Источники", Array.Empty<PortDefinition>(), GraphType.ListOf(GraphType.Int), (ctx, inputs) =>
        {
            var raw = ctx.Node.Settings.TryGetValue("value", out var s) ? s : "[ ]";
            var content = raw.Trim().Trim('[', ']');
            if (string.IsNullOrWhiteSpace(content))
                return EvaluationOutcome.Single(GraphValue.FromList(GraphType.Int, new List<GraphValue>()), "out");

            var parts = content.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            var list = new List<GraphValue>();
            foreach (var part in parts)
            {
                if (int.TryParse(part, NumberStyles.Integer, CultureInfo.InvariantCulture, out var iv))
                    list.Add(GraphValue.FromInt(iv));
                else if (double.TryParse(part, NumberStyles.Float, CultureInfo.InvariantCulture, out var dv))
                    list.Add(GraphValue.FromDouble(dv));
                else
                    return EvaluationOutcome.Error("Не удалось разобрать элемент списка");
            }

            var elementType = list.Any(v => v.Type.Kind == GraphTypeKind.Double) ? GraphType.Double : GraphType.Int;
            return EvaluationOutcome.Single(GraphValue.FromList(elementType, list), "out");
        }, "Литерал списка: формат [1,2,3]");
    }

    private static NodeDefinition BinaryInt(string code, string caption, Func<int, int, int> op)
    {
        var inputs = new[]
        {
            new PortDefinition("a", GraphType.Int),
            new PortDefinition("b", GraphType.Int)
        };

        return new LambdaNodeDefinition(code, caption, "Операторы", inputs, GraphType.Int, (ctx, values) =>
        {
            if (values.Any(v => v == null))
                return Partial(ctx, inputs, values, args => PerformBinary(args, op));
            return PerformBinary(values!, op);
        });
    }

    private static EvaluationOutcome PerformBinary(IReadOnlyList<GraphValue> values, Func<int, int, int> op)
    {
        var a = ToInt(values[0]);
        var b = ToInt(values[1]);
        return EvaluationOutcome.Single(GraphValue.FromInt(op(a, b)), "out");
    }

    private static NodeDefinition Divide()
    {
        var inputs = new[]
        {
            new PortDefinition("a", GraphType.Int),
            new PortDefinition("b", GraphType.Int)
        };

        return new LambdaNodeDefinition("op.div", "Деление", "Операторы", inputs, GraphType.Int, (ctx, values) =>
        {
            if (values.Any(v => v == null))
                return Partial(ctx, inputs, values, args => PerformDivide(args));
            return PerformDivide(values!);
        }, "Деление с проверкой деления на ноль");
    }

    private static EvaluationOutcome PerformDivide(IReadOnlyList<GraphValue> values)
    {
        var a = ToInt(values[0]);
        var b = ToInt(values[1]);
        if (b == 0) return EvaluationOutcome.Error("Деление на ноль");
        return EvaluationOutcome.Single(GraphValue.FromInt(a / b), "out");
    }

    private static NodeDefinition Not()
    {
        var inputs = new[] { new PortDefinition("значение", GraphType.Bool) };
        return new LambdaNodeDefinition("op.not", "Отрицание", "Операторы", inputs, GraphType.Bool, (ctx, values) =>
        {
            if (values.Any(v => v == null))
                return Partial(ctx, inputs, values, args => EvaluationOutcome.Single(GraphValue.FromBool(!args[0].Unwrap<bool>()), "out"));
            return EvaluationOutcome.Single(GraphValue.FromBool(!values[0]!.Unwrap<bool>()), "out");
        });
    }

    private static NodeDefinition Length()
    {
        var inputs = new[] { new PortDefinition("строка", GraphType.String) };
        return new LambdaNodeDefinition("op.len", "Длина строки", "Операторы", inputs, GraphType.Int, (ctx, values) =>
        {
            if (values[0] == null)
                return Partial(ctx, inputs, values, args => EvaluationOutcome.Single(GraphValue.FromInt(args[0].Unwrap<string>().Length), "out"));
            return EvaluationOutcome.Single(GraphValue.FromInt(values[0]!.Unwrap<string>().Length), "out");
        });
    }

    private static NodeDefinition ToUpper()
    {
        var inputs = new[] { new PortDefinition("строка", GraphType.String) };
        return new LambdaNodeDefinition("op.upper", "В верхний регистр", "Операторы", inputs, GraphType.String, (ctx, values) =>
        {
            if (values[0] == null)
                return Partial(ctx, inputs, values, args => EvaluationOutcome.Single(GraphValue.FromString(args[0].Unwrap<string>().ToUpperInvariant()), "out"));
            return EvaluationOutcome.Single(GraphValue.FromString(values[0]!.Unwrap<string>().ToUpperInvariant()), "out");
        });
    }

    private static NodeDefinition Compose()
    {
        var inputs = new[]
        {
            new PortDefinition("f", GraphType.FunctionOf(new []{ GraphType.Double }, GraphType.Double)),
            new PortDefinition("g", GraphType.FunctionOf(new []{ GraphType.Double }, GraphType.Double))
        };

        return new LambdaNodeDefinition("op.compose", "Композиция", "Высшие порядки", inputs, GraphType.FunctionOf(new []{ GraphType.Double }, GraphType.Double), (ctx, values) =>
        {
            if (values.Any(v => v == null))
                return Partial(ctx, inputs, values, args => BuildCompose(args[0], args[1]));
            return BuildCompose(values[0]!, values[1]!);
        }, "Композиция f∘g");
    }

    private static EvaluationOutcome BuildCompose(GraphValue fVal, GraphValue gVal)
    {
        if (fVal.Data is not FunctionValue f || gVal.Data is not FunctionValue g)
            return EvaluationOutcome.Error("Нужны функции");

        var composed = new FunctionValue(g.ArgumentTypes, f.ReturnType, args =>
        {
            var gRes = g.Invoke(args);
            if (gRes.HasError) return gRes;
            var gValue = gRes.Outputs.Values.First();
            var fRes = f.Invoke(new[] { gValue });
            return fRes;
        });

        return EvaluationOutcome.Single(GraphValue.FromFunction(composed), "out");
    }

    private static NodeDefinition Curry()
    {
        var inputs = new[]
        {
            new PortDefinition("функция", GraphType.FunctionOf(new []{ GraphType.Int, GraphType.Int }, GraphType.Int))
        };

        return new LambdaNodeDefinition("op.curry", "Каррирование", "Высшие порядки", inputs, GraphType.FunctionOf(new []{ GraphType.Int }, GraphType.FunctionOf(new []{ GraphType.Int }, GraphType.Int)), (ctx, values) =>
        {
            if (values[0] == null)
                return Partial(ctx, inputs, values, args => CurryImpl(args[0]));
            return CurryImpl(values[0]!);
        });
    }

    private static EvaluationOutcome CurryImpl(GraphValue fVal)
    {
        if (fVal.Data is not FunctionValue f || f.ArgumentTypes.Count < 2)
            return EvaluationOutcome.Error("Нужна функция двух аргументов");

        var curried = new FunctionValue(new[] { f.ArgumentTypes[0] }, GraphType.FunctionOf(new[] { f.ArgumentTypes[1] }, f.ReturnType), args =>
        {
            var first = args[0];
            var inner = new FunctionValue(new[] { f.ArgumentTypes[1] }, f.ReturnType, tail =>
            {
                var merged = new List<GraphValue> { first };
                merged.AddRange(tail);
                return f.Invoke(merged);
            });
            return EvaluationOutcome.Single(GraphValue.FromFunction(inner), "out");
        });

        return EvaluationOutcome.Single(GraphValue.FromFunction(curried), "out");
    }

    private static NodeDefinition Map()
    {
        var inputs = new[]
        {
            new PortDefinition("функция", GraphType.FunctionOf(new []{ GraphType.Int }, GraphType.Int)),
            new PortDefinition("список", GraphType.ListOf(GraphType.Int))
        };

        return new LambdaNodeDefinition("op.map", "Отображение списка", "Высшие порядки", inputs, GraphType.ListOf(GraphType.Int), (ctx, values) =>
        {
            if (values.Any(v => v == null))
                return Partial(ctx, inputs, values, args => MapImpl(args[0], args[1]));
            return MapImpl(values[0]!, values[1]!);
        });
    }

    private static EvaluationOutcome MapImpl(GraphValue funcValue, GraphValue listValue)
    {
        if (funcValue.Data is not FunctionValue func || listValue.Data is not IReadOnlyList<GraphValue> list)
            return EvaluationOutcome.Error("Требуются функция и список");

        var result = new List<GraphValue>();
        foreach (var el in list)
        {
            var res = func.Invoke(new[] { el });
            if (res.HasError) return res;
            result.Add(res.Outputs.Values.First());
        }

        return EvaluationOutcome.Single(GraphValue.FromList(func.ReturnType, result), "out");
    }

    private static NodeDefinition Filter()
    {
        var inputs = new[]
        {
            new PortDefinition("предикат", GraphType.FunctionOf(new []{ GraphType.Int }, GraphType.Bool)),
            new PortDefinition("список", GraphType.ListOf(GraphType.Int))
        };

        return new LambdaNodeDefinition("op.filter", "Фильтр списка", "Высшие порядки", inputs, GraphType.ListOf(GraphType.Int), (ctx, values) =>
        {
            if (values.Any(v => v == null))
                return Partial(ctx, inputs, values, args => FilterImpl(args[0], args[1]));
            return FilterImpl(values[0]!, values[1]!);
        });
    }

    private static EvaluationOutcome FilterImpl(GraphValue funcValue, GraphValue listValue)
    {
        if (funcValue.Data is not FunctionValue func || listValue.Data is not IReadOnlyList<GraphValue> list)
            return EvaluationOutcome.Error("Требуются функция и список");

        var result = new List<GraphValue>();
        foreach (var el in list)
        {
            var res = func.Invoke(new[] { el });
            if (res.HasError) return res;
            var value = res.Outputs.Values.First();
            if (value.Type.Kind != GraphTypeKind.Bool)
                return EvaluationOutcome.Error("Предикат должен возвращать Bool");
            var predicate = value.Unwrap<bool>();
            if (predicate) result.Add(el);
        }

        return EvaluationOutcome.Single(GraphValue.FromList(listValue.Type.ElementType ?? GraphType.Int, result), "out");
    }

    private static NodeDefinition Sum()
    {
        var inputs = new[] { new PortDefinition("список", GraphType.ListOf(GraphType.Int)) };
        return new LambdaNodeDefinition("op.sum", "Сумма", "Операторы", inputs, GraphType.Int, (ctx, values) =>
        {
            if (values[0] == null)
                return Partial(ctx, inputs, values, args => SumImpl(args[0]));
            return SumImpl(values[0]!);
        });
    }

    private static EvaluationOutcome SumImpl(GraphValue listValue)
    {
        if (listValue.Data is not IReadOnlyList<GraphValue> list)
            return EvaluationOutcome.Error("Требуется список");
        var total = list.Sum(v => ToInt(v));
        return EvaluationOutcome.Single(GraphValue.FromInt(total), "out");
    }

    private static NodeDefinition GreaterThan()
    {
        var inputs = new[] { new PortDefinition("порог", GraphType.Int) };
        return new LambdaNodeDefinition("op.gt", "Больше чем", "Высшие порядки", inputs, GraphType.FunctionOf(new[] { GraphType.Int }, GraphType.Bool), (ctx, values) =>
        {
            if (values[0] == null)
                return Partial(ctx, inputs, values, args => GreaterThanImpl(args[0]));
            return GreaterThanImpl(values[0]!);
        });
    }

    private static EvaluationOutcome GreaterThanImpl(GraphValue threshold)
    {
        var func = new FunctionValue(new[] { GraphType.Int }, GraphType.Bool, args =>
        {
            var res = args[0].Unwrap<int>() > threshold.Unwrap<int>();
            return EvaluationOutcome.Single(GraphValue.FromBool(res), "out");
        });
        return EvaluationOutcome.Single(GraphValue.FromFunction(func), "out");
    }

    private static EvaluationOutcome Partial(NodeExecutionContext ctx, IReadOnlyList<PortDefinition> ports, IReadOnlyList<GraphValue?> existing, Func<IReadOnlyList<GraphValue>, EvaluationOutcome> impl)
    {
        var captured = new List<GraphValue>();
        for (int i = 0; i < existing.Count; i++)
        {
            var v = existing[i];
            if (v != null && captured.Count == i)
                captured.Add(v);
            else
                break;
        }

        var fn = new FunctionValue(ports.Select(p => p.Type).ToList(), ctx.Node.Definition.Outputs.First().Type, impl, captured);
        return EvaluationOutcome.Single(GraphValue.FromFunction(fn), "out", "Частичное применение");
    }

    private static GraphValue? ParseValue(GraphType type, string raw)
    {
        return type.Kind switch
        {
            GraphTypeKind.Int => int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i) ? GraphValue.FromInt(i) : null,
            GraphTypeKind.Double => double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var d) ? GraphValue.FromDouble(d) : null,
            GraphTypeKind.String => GraphValue.FromString(raw),
            GraphTypeKind.Bool => bool.TryParse(raw, out var b) ? GraphValue.FromBool(b) : null,
            _ => null
        };
    }

    private static GraphValue DefaultValue(GraphType type)
    {
        return type.Kind switch
        {
            GraphTypeKind.Int => GraphValue.FromInt(0),
            GraphTypeKind.Double => GraphValue.FromDouble(0),
            GraphTypeKind.String => GraphValue.FromString(string.Empty),
            GraphTypeKind.Bool => GraphValue.FromBool(false),
            _ => GraphValue.FromString(string.Empty)
        };
    }

    private static int ToInt(GraphValue value)
    {
        return value.Type.Kind switch
        {
            GraphTypeKind.Int => value.Unwrap<int>(),
            GraphTypeKind.Double => (int)Math.Round(value.Unwrap<double>()),
            _ => 0
        };
    }
}
