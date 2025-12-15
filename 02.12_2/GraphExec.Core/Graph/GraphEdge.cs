namespace GraphExec.Core.Graph;

public sealed class GraphEdge
{
    public string FromNode { get; }
    public string FromPort { get; }
    public string ToNode { get; }
    public string ToPort { get; }

    public GraphEdge(string fromNode, string fromPort, string toNode, string toPort)
    {
        FromNode = fromNode;
        FromPort = fromPort;
        ToNode = toNode;
        ToPort = toPort;
    }
}
