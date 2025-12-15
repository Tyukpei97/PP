using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using GraphExec.Core.Graph;
using GraphExec.Core.Library;
using GraphExec.Core.Types;

namespace GraphExec.Core.Serialization;

public sealed class GraphDocumentDto
{
    public List<NodeDto> Nodes { get; set; } = new();
    public List<EdgeDto> Edges { get; set; } = new();
    public List<MacroDto> Macros { get; set; } = new();
}

public sealed class NodeDto
{
    public string Id { get; set; } = string.Empty;
    public string DefinitionCode { get; set; } = string.Empty;
    public double X { get; set; }
    public double Y { get; set; }
    public Dictionary<string, string>? Settings { get; set; }
    public TypeDto? OutputType { get; set; }
}

public sealed class EdgeDto
{
    public string FromNode { get; set; } = string.Empty;
    public string FromPort { get; set; } = "out";
    public string ToNode { get; set; } = string.Empty;
    public string ToPort { get; set; } = string.Empty;
}

public sealed class MacroDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public GraphDocumentDto Graph { get; set; } = new();
    public List<MacroInputDto> Inputs { get; set; } = new();
    public List<MacroOutputDto> Outputs { get; set; } = new();
}

public sealed class MacroInputDto
{
    public string Port { get; set; } = string.Empty;
    public string PlaceholderNodeId { get; set; } = string.Empty;
    public TypeDto? Type { get; set; }
}

public sealed class MacroOutputDto
{
    public string Port { get; set; } = string.Empty;
    public string SourceNode { get; set; } = string.Empty;
    public string SourcePort { get; set; } = "out";
}

/// <summary>
/// Сохранение/загрузка графа в JSON.
/// </summary>
public sealed class GraphSerializer
{
    private readonly Dictionary<string, NodeDefinition> _library;
    private readonly JsonSerializerOptions _options = new() { WriteIndented = true };

    public GraphSerializer(IEnumerable<NodeDefinition>? library = null)
    {
        _library = (library ?? NodeLibrary.All).ToDictionary(x => x.Code, x => x);
    }

    public string Save(GraphState state)
    {
        var dto = ToDto(state);
        return JsonSerializer.Serialize(dto, _options);
    }

    public GraphState Load(string json)
    {
        var dto = JsonSerializer.Deserialize<GraphDocumentDto>(json, _options) ?? new GraphDocumentDto();
        var macros = dto.Macros.Select(BuildMacro).ToList();
        var definitions = _library.Concat(macros.Select(m => new KeyValuePair<string, NodeDefinition>(m.Code, m)))
                                  .ToDictionary(k => k.Key, v => v.Value);

        var nodes = dto.Nodes.Select(n => new NodeInstance(n.Id, ResolveDefinition(n, definitions), n.X, n.Y, n.Settings ?? new())).ToList();
        var edges = dto.Edges.Select(e => new GraphEdge(e.FromNode, e.FromPort, e.ToNode, e.ToPort)).ToList();
        return new GraphState(nodes, edges, macros);
    }

    private GraphDocumentDto ToDto(GraphState state)
    {
        var dto = new GraphDocumentDto
        {
            Nodes = state.Nodes.Select(n => new NodeDto
            {
                Id = n.Id,
                DefinitionCode = n.Definition.Code,
                X = n.X,
                Y = n.Y,
                Settings = n.Settings.ToDictionary(k => k.Key, v => v.Value),
                OutputType = TypeCodec.ToDto(n.Definition.Outputs.First().Type)
            }).ToList(),
            Edges = state.Edges.Select(e => new EdgeDto { FromNode = e.FromNode, FromPort = e.FromPort, ToNode = e.ToNode, ToPort = e.ToPort }).ToList(),
            Macros = state.Macros.Select(ToDto).ToList()
        };

        return dto;
    }

    private MacroDto ToDto(MacroDefinition macro)
    {
        var graphDto = ToDto(macro.SubGraph);
        return new MacroDto
        {
            Code = macro.Code,
            Name = macro.DisplayName,
            Graph = graphDto,
            Inputs = macro.InputBindings.Select(i => new MacroInputDto
            {
                Port = i.PortName,
                PlaceholderNodeId = i.PlaceholderNodeId,
                Type = TypeCodec.ToDto(i.Type)
            }).ToList(),
            Outputs = macro.OutputBindings.Select(o => new MacroOutputDto
            {
                Port = o.PortName,
                SourceNode = o.SourceNodeId,
                SourcePort = o.SourcePort
            }).ToList()
        };
    }

    private MacroDefinition BuildMacro(MacroDto dto)
    {
        var sub = LoadSubGraph(dto.Graph);
        var inputs = dto.Inputs.Select(i => new MacroInputBinding(i.Port, i.PlaceholderNodeId, TypeCodec.FromDto(i.Type!))).ToList();
        var outputs = dto.Outputs.Select(o => new MacroOutputBinding(o.Port, o.SourceNode, o.SourcePort)).ToList();
        return new MacroDefinition(dto.Code, dto.Name, sub, inputs, outputs);
    }

    private GraphState LoadSubGraph(GraphDocumentDto dto)
    {
        var nodes = dto.Nodes.Select(n => new NodeInstance(n.Id, ResolveDefinition(n, _library), n.X, n.Y, n.Settings ?? new())).ToList();
        var edges = dto.Edges.Select(e => new GraphEdge(e.FromNode, e.FromPort, e.ToNode, e.ToPort)).ToList();
        return new GraphState(nodes, edges);
    }

    private NodeDefinition ResolveDefinition(NodeDto dto, IReadOnlyDictionary<string, NodeDefinition> map)
    {
        if (dto.DefinitionCode.StartsWith("macro.input"))
        {
            var type = dto.OutputType != null ? TypeCodec.FromDto(dto.OutputType) : GraphType.String;
            return new MacroInputDefinition(type, dto.Id);
        }

        if (map.TryGetValue(dto.DefinitionCode, out var def))
            return def;

        return NodeLibrary.All.First();
    }
}
