using System.Collections.Generic;
using Topology.Core.Models;
using Topology.Core;

namespace Topology.Core.History;

public class HistoryManager
{
    private readonly Stack<TopologySnapshot> _undo = new();
    private readonly Stack<TopologySnapshot> _redo = new();

    public void Record(TopologySpace space)
    {
        _undo.Push(TopologySnapshot.FromSpace(space));
        _redo.Clear();
    }

    public bool CanUndo => _undo.Count > 0;
    public bool CanRedo => _redo.Count > 0;

    public bool TryUndo(TopologySpace space)
    {
        if (!CanUndo) return false;
        var snapshot = _undo.Pop();
        _redo.Push(TopologySnapshot.FromSpace(space));
        Apply(space, snapshot);
        return true;
    }

    public bool TryRedo(TopologySpace space)
    {
        if (!CanRedo) return false;
        var snapshot = _redo.Pop();
        _undo.Push(TopologySnapshot.FromSpace(space));
        Apply(space, snapshot);
        return true;
    }

    private void Apply(TopologySpace space, TopologySnapshot snapshot)
    {
        space.Points.Clear();
        space.OpenSets.Clear();
        foreach (var p in snapshot.Points)
            space.Points.Add(new TopologyPoint { Id = p.Id, Name = p.Name, X = p.X, Y = p.Y });
        foreach (var o in snapshot.OpenSets)
            space.OpenSets.Add(new OpenSet
            {
                Name = o.Name,
                Mask = o.Mask,
                ColorHex = o.ColorHex,
                Opacity = o.Opacity,
                IsVisible = o.IsVisible
            });
    }
}
