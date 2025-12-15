namespace Topology.Core.Models;

public class ContinuityResult
{
    public bool IsContinuous { get; set; }
    public List<string> Issues { get; } = new();
}
