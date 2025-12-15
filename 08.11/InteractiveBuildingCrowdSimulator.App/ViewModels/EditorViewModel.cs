using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using InteractiveBuildingCrowdSimulator.App.Infrastructure;
using InteractiveBuildingCrowdSimulator.App.Models;

namespace InteractiveBuildingCrowdSimulator.App.ViewModels;

public enum EditorTool
{
    Select,
    AddRoom,
    AddCorridor,
    AddStair,
    AddDoor,
    AddObstacle
}

/// <summary>
/// Вью-модель редактора карты здания.
/// </summary>
public class EditorViewModel : ObservableObject
{
    private readonly RelayCommand _removeCommand;
    private readonly RelayCommand _validateCommand;
    private readonly RelayCommand _toggleSnapCommand;
    private readonly RelayCommand _doorCommand;

    private Area? _selectedArea;
    private Area? _doorStart;
    private bool _snapToGrid = true;
    private double _gridSize = 1.0;
    private string _statusMessage = "Редактор готов";
    private EditorTool _selectedTool = EditorTool.Select;

    public EditorViewModel(BuildingMap map)
    {
        Map = map;

        _removeCommand = new RelayCommand(_ => RemoveSelected(), _ => SelectedArea != null);
        _validateCommand = new RelayCommand(_ => ValidateConnectivity());
        _toggleSnapCommand = new RelayCommand(_ => SnapToGrid = !SnapToGrid);
        _doorCommand = new RelayCommand(_ => SelectDoorStart(), _ => SelectedTool == EditorTool.AddDoor);
    }

    public BuildingMap Map { get; }

    public Area? SelectedArea
    {
        get => _selectedArea;
        set
        {
            if (SetProperty(ref _selectedArea, value))
            {
                StatusMessage = value is null ? "Ничего не выбрано" : $"Выбрано: {value.Name}";
                _removeCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool SnapToGrid
    {
        get => _snapToGrid;
        set => SetProperty(ref _snapToGrid, value);
    }

    public double GridSize
    {
        get => _gridSize;
        set => SetProperty(ref _gridSize, Math.Max(0.5, value));
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public EditorTool SelectedTool
    {
        get => _selectedTool;
        set => SetProperty(ref _selectedTool, value);
    }

    public ICommand RemoveCommand => _removeCommand;
    public ICommand ValidateCommand => _validateCommand;
    public ICommand ToggleSnapCommand => _toggleSnapCommand;
    public ICommand DoorCommand => _doorCommand;

    public void HandleCanvasClick(Point position)
    {
        var snapped = Snap(position);
        switch (SelectedTool)
        {
            case EditorTool.AddRoom:
                AddRoom(snapped);
                break;
            case EditorTool.AddCorridor:
                AddCorridor(snapped);
                break;
            case EditorTool.AddStair:
                AddStair(snapped);
                break;
            case EditorTool.AddObstacle:
                AddObstacle(snapped);
                break;
            case EditorTool.AddDoor:
                SelectDoorArea(snapped);
                break;
            default:
                SelectedArea = HitTest(snapped);
                break;
        }
    }

    public void MoveSelected(Vector delta)
    {
        if (SelectedArea is null)
        {
            return;
        }

        var rect = SelectedArea.Bounds;
        rect.X += delta.X;
        rect.Y += delta.Y;
        SelectedArea.Bounds = rect;
        OnPropertyChanged(nameof(SelectedArea));
    }

    private void AddRoom(Point position)
    {
        var rect = new Rect(position.X, position.Y, 8, 6);
        var room = new Room
        {
            Name = $"Комната {Map.Rooms.Count + 1}",
            Bounds = rect,
            Capacity = 30
        };

        Map.Rooms.Add(room);
        SelectedArea = room;
        StatusMessage = "Комната добавлена";
    }

    private void AddCorridor(Point position)
    {
        var rect = new Rect(position.X, position.Y, 10, 3);
        var corridor = new Corridor
        {
            Name = $"Коридор {Map.Corridors.Count + 1}",
            Bounds = rect,
            PassageWidth = 2
        };

        Map.Corridors.Add(corridor);
        SelectedArea = corridor;
        StatusMessage = "Коридор добавлен";
    }

    private void AddStair(Point position)
    {
        var rect = new Rect(position.X, position.Y, 4, 4);
        var stair = new Stair
        {
            Name = $"Лестница {Map.Stairs.Count + 1}",
            Bounds = rect,
            Levels = 2
        };

        Map.Stairs.Add(stair);
        SelectedArea = stair;
        StatusMessage = "Лестница добавлена";
    }

    private void AddObstacle(Point position)
    {
        var rect = new Rect(position.X, position.Y, 3, 1);
        Map.Obstacles.Add(new Obstacle { Bounds = rect });
        StatusMessage = "Препятствие добавлено";
    }

    private void SelectDoorArea(Point position)
    {
        var area = HitTest(position);
        if (area is null)
        {
            StatusMessage = "Выберите две области для соединения дверью";
            return;
        }

        if (_doorStart is null)
        {
            _doorStart = area;
            StatusMessage = $"Начало двери: {area.Name}. Выберите вторую область.";
        }
        else
        {
            if (_doorStart.Id == area.Id)
            {
                StatusMessage = "Нужны разные области для двери.";
                return;
            }

            Map.Doors.Add(new Door
            {
                FromAreaId = _doorStart.Id,
                ToAreaId = area.Id,
                Width = 1.2,
                ThroughputPerSecond = 2,
                OneWay = false
            });

            StatusMessage = $"Дверь между {_doorStart.Name} и {area.Name} создана";
            _doorStart = null;
        }
    }

    private void RemoveSelected()
    {
        if (SelectedArea is null)
        {
            return;
        }

        var id = SelectedArea.Id;
        if (SelectedArea is Room room)
        {
            Map.Rooms.Remove(room);
        }
        else if (SelectedArea is Corridor corridor)
        {
            Map.Corridors.Remove(corridor);
        }
        else if (SelectedArea is Stair stair)
        {
            Map.Stairs.Remove(stair);
        }
        for (int i = Map.Doors.Count - 1; i >= 0; i--)
        {
            if (Map.Doors[i].FromAreaId == id || Map.Doors[i].ToAreaId == id)
            {
                Map.Doors.RemoveAt(i);
            }
        }
        SelectedArea = null;
        StatusMessage = "Объект удален";
    }

    private void ValidateConnectivity()
    {
        if (!Map.AllAreas.Any())
        {
            StatusMessage = "Нет областей для проверки.";
            return;
        }

        var start = Map.AllAreas.First();
        var visited = Map.AllAreas.ToDictionary(a => a.Id, _ => false);
        Visit(start.Id, visited);

        var disconnected = visited.Where(v => !v.Value).Select(v => Map.FindArea(v.Key)?.Name ?? v.Key.ToString()).ToList();
        StatusMessage = disconnected.Any()
            ? $"Внимание: нет пути к {string.Join(", ", disconnected)}"
            : "Все области связаны дверями";
    }

    private void Visit(Guid id, System.Collections.Generic.IDictionary<Guid, bool> visited)
    {
        if (visited.TryGetValue(id, out var already) && already)
        {
            return;
        }

        visited[id] = true;
        foreach (var neighbor in Map.Doors.Where(d => d.FromAreaId == id || (!d.OneWay && d.ToAreaId == id)))
        {
            var next = neighbor.FromAreaId == id ? neighbor.ToAreaId : neighbor.FromAreaId;
            Visit(next, visited);
        }
    }

    private Point Snap(Point position)
    {
        if (!SnapToGrid)
        {
            return position;
        }

        var gx = Math.Round(position.X / GridSize) * GridSize;
        var gy = Math.Round(position.Y / GridSize) * GridSize;
        return new Point(gx, gy);
    }

    private Area? HitTest(Point position)
    {
        return Map.AllAreas.LastOrDefault(a => a.Bounds.Contains(position));
    }

    private void SelectDoorStart()
    {
        _doorStart = null;
        StatusMessage = "Щелкните по первой области, затем по второй для создания двери";
    }
}
