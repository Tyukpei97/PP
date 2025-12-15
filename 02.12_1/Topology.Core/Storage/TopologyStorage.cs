using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Topology.Core.Models;

namespace Topology.Core.Storage;

public static class TopologyStorage
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static void Save(string path, TopologyProject project)
    {
        var json = JsonSerializer.Serialize(project, Options);
        File.WriteAllText(path, json);
    }

    public static TopologyProject Load(string path)
    {
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<TopologyProject>(json, Options)
               ?? new TopologyProject();
    }
}
