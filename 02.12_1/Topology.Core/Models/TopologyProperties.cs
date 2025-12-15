namespace Topology.Core.Models;

public class TopologyProperties
{
    public bool IsT0 { get; set; }
    public bool IsT1 { get; set; }
    public bool IsT2 { get; set; }
    public bool IsConnected { get; set; }
    public bool IsCompact { get; set; }
    public Dictionary<int, int> MinimalNeighborhoods { get; set; } = new();
    public string CompactnessExplanation { get; set; } = "Любое конечное пространство компактно.";
}
