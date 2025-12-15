using GraphExec.Core.Graph;
using GraphExec.Core.Types;

namespace GraphExec.UI.ViewModels;

public sealed class PortViewModel
{
    public string Name { get; }
    public GraphType Type { get; }
    public int Index { get; }
    public bool IsInput { get; }
    public NodeViewModel Owner { get; }

    public PortViewModel(NodeViewModel owner, string name, GraphType type, int index, bool isInput)
    {
        Owner = owner;
        Name = name;
        Type = type;
        Index = index;
        IsInput = isInput;
    }
}
