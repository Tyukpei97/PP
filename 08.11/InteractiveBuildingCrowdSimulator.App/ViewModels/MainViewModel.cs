using System;
using System.IO;
using System.Linq;
using System.Windows.Input;
using InteractiveBuildingCrowdSimulator.App.Infrastructure;
using InteractiveBuildingCrowdSimulator.App.Models;
using InteractiveBuildingCrowdSimulator.App.Services;
using Microsoft.Win32;

namespace InteractiveBuildingCrowdSimulator.App.ViewModels;

/// <summary>
/// Главная вью-модель: навигация между разделами и импорт/экспорт.
/// </summary>
public class MainViewModel : ObservableObject
{
    private readonly SerializationService _serialization;
    private object? _currentViewModel;
    private string _status = "Готово";

    public MainViewModel()
    {
        var map = new BuildingMap();
        var analytics = new AnalyticsViewModel();
        var pathfinding = new PathfindingService();
        var avoidance = new CollisionAvoidanceService();
        var simulationEngine = new SimulationEngine(pathfinding, avoidance);
        var statistics = new StatisticsService();

        Editor = new EditorViewModel(map);
        Analytics = analytics;
        Simulation = new SimulationViewModel(simulationEngine, statistics, analytics, map);
        AgentDetails = new AgentDetailsViewModel();

        _serialization = new SerializationService();

        ShowEditorCommand = new RelayCommand(_ => CurrentViewModel = Editor);
        ShowSimulationCommand = new RelayCommand(_ => CurrentViewModel = Simulation);
        ShowAnalyticsCommand = new RelayCommand(_ => CurrentViewModel = Analytics);
        ExportCommand = new RelayCommand(_ => Export());
        ImportCommand = new RelayCommand(_ => Import());
        LoadSampleCommand = new RelayCommand(_ => LoadSample());

        CurrentViewModel = Editor;
    }

    public EditorViewModel Editor { get; }
    public SimulationViewModel Simulation { get; }
    public AnalyticsViewModel Analytics { get; }
    public AgentDetailsViewModel AgentDetails { get; }

    public object? CurrentViewModel
    {
        get => _currentViewModel;
        set => SetProperty(ref _currentViewModel, value);
    }

    public string Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    public ICommand ShowEditorCommand { get; }
    public ICommand ShowSimulationCommand { get; }
    public ICommand ShowAnalyticsCommand { get; }
    public ICommand ExportCommand { get; }
    public ICommand ImportCommand { get; }
    public ICommand LoadSampleCommand { get; }

    private async void Export()
    {
        var dlg = new SaveFileDialog
        {
            Title = "Экспорт карты в JSON",
            Filter = "JSON|*.json",
            InitialDirectory = GetUserDataFolder()
        };

        if (dlg.ShowDialog() != true)
        {
            return;
        }

        await _serialization.SaveAsync(dlg.FileName, Editor.Map, Simulation.Settings);
        Status = $"Карта сохранена в {dlg.FileName}";
    }

    private async void Import()
    {
        var dlg = new OpenFileDialog
        {
            Title = "Импорт карты из JSON",
            Filter = "JSON|*.json",
            InitialDirectory = GetUserDataFolder()
        };

        if (dlg.ShowDialog() != true)
        {
            return;
        }

        var (map, settings) = await _serialization.LoadAsync(dlg.FileName);
        Editor.Map.Rooms.Clear();
        Editor.Map.Corridors.Clear();
        Editor.Map.Stairs.Clear();
        Editor.Map.Doors.Clear();
        Editor.Map.Obstacles.Clear();

        foreach (var r in map.Rooms) Editor.Map.Rooms.Add(r);
        foreach (var c in map.Corridors) Editor.Map.Corridors.Add(c);
        foreach (var s in map.Stairs) Editor.Map.Stairs.Add(s);
        foreach (var d in map.Doors) Editor.Map.Doors.Add(d);
        foreach (var o in map.Obstacles) Editor.Map.Obstacles.Add(o);

        Simulation.Settings.AgentCount = settings.AgentCount;
        Simulation.Settings.MinSpeed = settings.MinSpeed;
        Simulation.Settings.MaxSpeed = settings.MaxSpeed;
        Simulation.Settings.MinStress = settings.MinStress;
        Simulation.Settings.MaxStress = settings.MaxStress;
        Simulation.Settings.Type = settings.Type;
        Simulation.Settings.TargetAreaId = settings.TargetAreaId;
        Simulation.SelectedScenario = Simulation.Scenarios.FirstOrDefault(s => s.Type == settings.Type);

        Status = $"Импортировано: {dlg.FileName}";
    }

    private void LoadSample()
    {
        Editor.Map.Rooms.Clear();
        Editor.Map.Corridors.Clear();
        Editor.Map.Stairs.Clear();
        Editor.Map.Doors.Clear();
        Editor.Map.Obstacles.Clear();

        var room1 = new Room { Name = "Офис", Bounds = new System.Windows.Rect(5, 5, 12, 10), Capacity = 30 };
        var room2 = new Room { Name = "Переговорная", Bounds = new System.Windows.Rect(22, 5, 10, 8), Capacity = 15 };
        var corridor = new Corridor { Name = "Коридор", Bounds = new System.Windows.Rect(4, 18, 30, 4), PassageWidth = 2.5 };
        var exit = new Room { Name = "Выход", Bounds = new System.Windows.Rect(38, 16, 6, 6), Capacity = 100 };

        Editor.Map.Rooms.Add(room1);
        Editor.Map.Rooms.Add(room2);
        Editor.Map.Rooms.Add(exit);
        Editor.Map.Corridors.Add(corridor);
        Editor.Map.Doors.Add(new Door { FromAreaId = room1.Id, ToAreaId = corridor.Id, Width = 1.5, ThroughputPerSecond = 3 });
        Editor.Map.Doors.Add(new Door { FromAreaId = room2.Id, ToAreaId = corridor.Id, Width = 1.2, ThroughputPerSecond = 2.5 });
        Editor.Map.Doors.Add(new Door { FromAreaId = corridor.Id, ToAreaId = exit.Id, Width = 1.8, ThroughputPerSecond = 4 });

        Simulation.Settings.TargetAreaId = exit.Id;
        Status = "Пример карты загружен";
    }

    private string GetUserDataFolder()
    {
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CrowdSimulator");
        Directory.CreateDirectory(path);
        return path;
    }
}
