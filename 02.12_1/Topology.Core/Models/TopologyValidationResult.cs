using System.Collections.Generic;

namespace Topology.Core.Models;

public class TopologyValidationResult
{
    public bool IsValid { get; set; }
    public List<int> Missing { get; } = new();
    public List<int> AddedByClosure { get; } = new();
    public List<string> Issues { get; } = new();
}
