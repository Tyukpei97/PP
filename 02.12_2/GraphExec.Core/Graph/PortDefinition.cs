using GraphExec.Core.Types;

namespace GraphExec.Core.Graph;

public sealed class PortDefinition
{
    public string Name { get; }
    public GraphType Type { get; }
    public bool AllowMultiple { get; }

    public PortDefinition(string name, GraphType type, bool allowMultiple = false)
    {
        Name = name;
        Type = type;
        AllowMultiple = allowMultiple;
    }
}
