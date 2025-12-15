using System.Linq;
using Topology.Core;

namespace Topology.Core.Models;

public class TopologySnapshot
{
    public List<TopologyPoint> Points { get; set; } = new();
    public List<OpenSet> OpenSets { get; set; } = new();

    public static TopologySnapshot FromSpace(TopologySpace space)
    {
        return new TopologySnapshot
        {
            Points = space.Points.Select(p => new TopologyPoint
            {
                Id = p.Id,
                Name = p.Name,
                X = p.X,
                Y = p.Y
            }).ToList(),
            OpenSets = space.OpenSets.Select(o => new OpenSet
            {
                Name = o.Name,
                Mask = o.Mask,
                ColorHex = o.ColorHex,
                Opacity = o.Opacity,
                IsVisible = o.IsVisible
            }).ToList()
        };
    }
}
