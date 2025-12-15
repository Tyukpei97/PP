using System.Collections.Generic;
using System.Linq;
using GraphExec.Core.Types;

namespace GraphExec.Core.Graph;

public sealed class GraphValidator
{
    public IReadOnlyList<string> Validate(GraphState graph)
    {
        var errors = new List<string>();
        errors.AddRange(ValidateCycles(graph));
        errors.AddRange(ValidateTypes(graph));
        return errors;
    }

    private IEnumerable<string> ValidateCycles(GraphState graph)
    {
        var visited = new Dictionary<string, int>(); // 0-not,1-visiting,2-done

        bool Dfs(string nodeId)
        {
            visited[nodeId] = 1;
            foreach (var edge in graph.OutgoingFrom(nodeId))
            {
                if (!visited.ContainsKey(edge.ToNode))
                {
                    if (Dfs(edge.ToNode))
                        return true;
                }
                else if (visited[edge.ToNode] == 1)
                {
                    return true;
                }
            }

            visited[nodeId] = 2;
            return false;
        }

        foreach (var node in graph.Nodes)
        {
            if (!visited.ContainsKey(node.Id) && Dfs(node.Id))
                yield return $"Обнаружен цикл с участием узла {node.Definition.DisplayName}";
        }
    }

    private IEnumerable<string> ValidateTypes(GraphState graph)
    {
        foreach (var edge in graph.Edges)
        {
            var from = graph.FindNode(edge.FromNode);
            var to = graph.FindNode(edge.ToNode);
            if (from == null || to == null)
            {
                yield return "Обнаружено соединение с отсутствующим узлом";
                continue;
            }

            var fromPort = from.Definition.Outputs.FirstOrDefault(p => p.Name == edge.FromPort);
            var toPort = to.Definition.Inputs.FirstOrDefault(p => p.Name == edge.ToPort);
            if (fromPort == null || toPort == null)
            {
                yield return $"Некорректное имя порта в соединении {edge.FromNode}->{edge.ToNode}";
                continue;
            }

            if (!toPort.Type.IsAssignableFrom(fromPort.Type))
                yield return $"Тип {fromPort.Type.DisplayName} не совместим с {toPort.Type.DisplayName} ({from.Definition.DisplayName} -> {to.Definition.DisplayName})";
        }
    }

    public IReadOnlyList<NodeInstance> TopologicalSort(GraphState graph)
    {
        var inDegree = graph.Nodes.ToDictionary(n => n.Id, _ => 0);
        foreach (var edge in graph.Edges)
        {
            if (inDegree.ContainsKey(edge.ToNode))
                inDegree[edge.ToNode]++;
        }

        var queue = new Queue<NodeInstance>(graph.Nodes.Where(n => inDegree[n.Id] == 0));
        var result = new List<NodeInstance>();
        while (queue.Count > 0)
        {
            var n = queue.Dequeue();
            result.Add(n);
            foreach (var edge in graph.OutgoingFrom(n.Id))
            {
                if (!inDegree.ContainsKey(edge.ToNode))
                    continue;
                inDegree[edge.ToNode]--;
                if (inDegree[edge.ToNode] == 0)
                    queue.Enqueue(graph.FindNode(edge.ToNode)!);
            }
        }

        return result;
    }
}
