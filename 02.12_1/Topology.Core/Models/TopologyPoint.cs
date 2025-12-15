namespace Topology.Core.Models;

/// <summary>
/// Абстрактная точка пространства с позицией для визуализации.
/// </summary>
public class TopologyPoint
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double X { get; set; }
    public double Y { get; set; }
}
