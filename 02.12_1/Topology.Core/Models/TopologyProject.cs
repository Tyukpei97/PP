namespace Topology.Core.Models;

/// <summary>
/// DTO для импорта/экспорта проекта.
/// </summary>
public class TopologyProject
{
    public List<TopologyPoint> Points { get; set; } = new();
    public List<OpenSet> OpenSets { get; set; } = new();
    public string? Title { get; set; }
}
