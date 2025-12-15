using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using InteractiveBuildingCrowdSimulator.App.Models;

namespace InteractiveBuildingCrowdSimulator.App.Services;

/// <summary>
/// Экспорт/импорт карты здания и настроек в JSON (только для этой функции).
/// </summary>
public class SerializationService
{
    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true
    };

    public async Task SaveAsync(string path, BuildingMap map, ScenarioSettings settings)
    {
        var dto = new MapFileDto(map, settings);
        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, dto, _options);
    }

    public async Task<(BuildingMap map, ScenarioSettings settings)> LoadAsync(string path)
    {
        await using var stream = File.OpenRead(path);
        var dto = await JsonSerializer.DeserializeAsync<MapFileDto>(stream, _options)
                  ?? new MapFileDto(new BuildingMap(), new ScenarioSettings());
        return (dto.Map, dto.Settings);
    }

    private record MapFileDto(BuildingMap Map, ScenarioSettings Settings);
}
