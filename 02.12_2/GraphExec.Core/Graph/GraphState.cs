using System.Collections.Generic;
using System.Linq;

namespace GraphExec.Core.Graph;

public sealed class GraphState
{
    public IReadOnlyList<NodeInstance> Nodes { get; }
    public IReadOnlyList<GraphEdge> Edges { get; }
    public IReadOnlyList<MacroDefinition> Macros { get; }

    public GraphState(IEnumerable<NodeInstance>? nodes = null, IEnumerable<GraphEdge>? edges = null, IEnumerable<MacroDefinition>? macros = null)
    {
        Nodes = nodes?.ToList() ?? new List<NodeInstance>();
        Edges = edges?.ToList() ?? new List<GraphEdge>();
        Macros = macros?.ToList() ?? new List<MacroDefinition>();
    }

    public NodeInstance? FindNode(string id) => Nodes.FirstOrDefault(n => n.Id == id);
    public IEnumerable<GraphEdge> IncomingTo(string nodeId) => Edges.Where(e => e.ToNode == nodeId);
    public IEnumerable<GraphEdge> OutgoingFrom(string nodeId) => Edges.Where(e => e.FromNode == nodeId);

    public GraphState WithNodes(IEnumerable<NodeInstance> nodes) => new(nodes, Edges, Macros);
    public GraphState WithEdges(IEnumerable<GraphEdge> edges) => new(Nodes, edges, Macros);
    public GraphState WithMacros(IEnumerable<MacroDefinition> macros) => new(Nodes, Edges, macros);
}
