using System.Collections.Generic;

namespace GraphExec.Core.Graph;

public sealed class NodeInstance
{
    public string Id { get; }
    public NodeDefinition Definition { get; }
    public double X { get; }
    public double Y { get; }
    public IReadOnlyDictionary<string, string> Settings { get; }

    public NodeInstance(string id, NodeDefinition definition, double x, double y, IReadOnlyDictionary<string, string>? settings = null)
    {
        Id = id;
        Definition = definition;
        X = x;
        Y = y;
        Settings = settings ?? new Dictionary<string, string>();
    }

    public NodeInstance Move(double x, double y) => new(Id, Definition, x, y, Settings);
    public NodeInstance WithSettings(IReadOnlyDictionary<string, string> settings) => new(Id, Definition, X, Y, settings);
}
