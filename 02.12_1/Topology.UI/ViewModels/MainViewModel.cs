using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.Win32;
using Topology.Core;
using Topology.Core.Examples;
using Topology.Core.History;
using Topology.Core.Models;
using Topology.Core.Storage;

namespace Topology.UI.ViewModels;

public class MainViewModel : BaseViewModel
{
    private readonly HistoryManager _history = new();

    public TopologySpace Space { get; } = new();
    public TopologySpace TargetSpace { get; private set; } = new();

    public ObservableCollection<int> SelectedPoints { get; } = new();
    public ObservableCollection<PointMappingViewModel> PointMappings { get; } = new();

    private OpenSet? _selectedOpenSet;
    public OpenSet? SelectedOpenSet
    {
        get => _selectedOpenSet;
        set
        {
            if (_selectedOpenSet == value) return;
            _selectedOpenSet = value;
            OnPropertyChanged();
        }
    }

    private string _validationMessage = "Добавьте точки и открытые множества.";
    public string ValidationMessage
    {
        get => _validationMessage;
        set
        {
            _validationMessage = value;
            OnPropertyChanged();
        }
    }

    private string _continuityMessage = "Настройте второе пространство и отображение.";
    public string ContinuityMessage
    {
        get => _continuityMessage;
        set
        {
            _continuityMessage = value;
            OnPropertyChanged();
        }
    }

    private string _newOpenSetName = "U1";
    public string NewOpenSetName
    {
        get => _newOpenSetName;
        set
        {
            _newOpenSetName = value;
            OnPropertyChanged();
        }
    }

    private string _selectedPointName = string.Empty;
    public string SelectedPointName
    {
        get => _selectedPointName;
        set
        {
            if (_selectedPointName == value) return;
            _selectedPointName = value;
            RenameSelectedPoint(value);
            OnPropertyChanged();
        }
    }

    private TopologyProperties? _properties;
    public TopologyProperties? Properties
    {
        get => _properties;
        set
        {
            _properties = value;
            OnPropertyChanged();
        }
    }

    public ICommand AutoClosureCommand { get; }
    public ICommand ValidateCommand { get; }
    public ICommand AddOpenSetCommand { get; }
    public ICommand DeleteOpenSetCommand { get; }
    public ICommand ImportCommand { get; }
    public ICommand ExportCommand { get; }
    public ICommand UndoCommand { get; }
    public ICommand RedoCommand { get; }
    public ICommand GenerateExampleCommand { get; }
    public ICommand GenerateTargetExampleCommand { get; }
    public ICommand CheckContinuityCommand { get; }
    public ICommand ImportTargetCommand { get; }

    public MainViewModel()
    {
        Space.Points.CollectionChanged += (_, _) => OnPointsChanged();
        Space.OpenSets.CollectionChanged += (_, _) => ValidateTopology();

        AutoClosureCommand = new RelayCommand(_ => AutoClose());
        ValidateCommand = new RelayCommand(_ => ValidateTopology());
        AddOpenSetCommand = new RelayCommand(_ => AddOpenSetFromSelection(), _ => SelectedPoints.Any());
        DeleteOpenSetCommand = new RelayCommand(obj =>
        {
            if (obj is OpenSet set)
            {
                _history.Record(Space);
                RefreshCommands();
                Space.DeleteOpenSet(set);
                ValidateTopology();
            }
        });
        ImportCommand = new RelayCommand(_ => ImportSpace());
        ExportCommand = new RelayCommand(_ => ExportSpace());
        UndoCommand = new RelayCommand(_ =>
        {
            if (_history.TryUndo(Space))
            {
                ValidateTopology();
                RefreshCommands();
            }
        }, _ => _history.CanUndo);
        RedoCommand = new RelayCommand(_ =>
        {
            if (_history.TryRedo(Space))
            {
                ValidateTopology();
                RefreshCommands();
            }
        }, _ => _history.CanRedo);
        GenerateExampleCommand = new RelayCommand(key => GenerateExample(key?.ToString()));
        GenerateTargetExampleCommand = new RelayCommand(key => GenerateTargetSpace(key?.ToString()));
        ImportTargetCommand = new RelayCommand(_ => ImportTargetSpace());
        CheckContinuityCommand = new RelayCommand(_ => CheckContinuity());

        ValidateTopology();
    }

    public void AddPointAt(double x, double y)
    {
        _history.Record(Space);
        RefreshCommands();
        Space.AddPoint(null, x, y);
        UpdateMappings();
        ValidateTopology();
    }

    public void MovePoint(int id, double x, double y)
    {
        var point = Space.Points.FirstOrDefault(p => p.Id == id);
        if (point == null) return;
        point.X = x;
        point.Y = y;
        OnPropertyChanged(nameof(Space));
    }

    public void BeginPointMove()
    {
        _history.Record(Space);
        RefreshCommands();
    }

    public void ToggleSelectPoint(int id)
    {
        if (SelectedPoints.Contains(id))
            SelectedPoints.Remove(id);
        else
            SelectedPoints.Add(id);

        var first = SelectedPoints.FirstOrDefault();
        var point = Space.Points.FirstOrDefault(p => p.Id == first);
        SelectedPointName = point?.Name ?? string.Empty;
        OnPropertyChanged(nameof(SelectedPoints));
        RefreshCommands();
    }

    public void DeleteSelectedPoints()
    {
        if (!SelectedPoints.Any()) return;
        _history.Record(Space);
        RefreshCommands();
        foreach (var id in SelectedPoints.ToList())
            Space.RemovePoint(id);
        SelectedPoints.Clear();
        UpdateMappings();
        ValidateTopology();
        RefreshCommands();
    }

    private void RenameSelectedPoint(string newName)
    {
        if (!SelectedPoints.Any()) return;
        var id = SelectedPoints.First();
        Space.RenamePoint(id, newName);
        OnPropertyChanged(nameof(Space));
    }

    private void AddOpenSetFromSelection()
    {
        _history.Record(Space);
        RefreshCommands();
        var mask = Space.ToMask(SelectedPoints);
        var name = string.IsNullOrWhiteSpace(NewOpenSetName) ? $"U{Space.OpenSets.Count + 1}" : NewOpenSetName;
        var set = Space.CreateOpenSet(name, mask);
        set.ColorHex ??= GenerateColor();
        NewOpenSetName = $"U{Space.OpenSets.Count + 1}";
        ValidateTopology();
    }

    private void AutoClose()
    {
        _history.Record(Space);
        RefreshCommands();
        var result = Space.ComputeClosure(mutate: true);
        ValidateTopology();
        ValidationMessage = result.Missing.Any()
            ? $"Добавлены минимальные множества: {result.AddedByClosure.Count}"
            : "Топология уже замкнута.";
    }

    private void ValidateTopology()
    {
        var validation = Space.ComputeClosure(mutate: false);
        if (validation.IsValid)
        {
            ValidationMessage = "Все аксиомы топологии выполнены.";
            Properties = Space.ComputeProperties();
        }
        else
        {
            ValidationMessage = string.Join(" ", validation.Issues);
            Properties = null;
        }
        OnPropertyChanged(nameof(Properties));
    }

    private void ImportSpace()
    {
        var dlg = new OpenFileDialog
        {
            Filter = "JSON (*.json)|*.json|Все файлы|*.*",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
        };
        if (dlg.ShowDialog() == true)
        {
            var project = TopologyStorage.Load(dlg.FileName);
            var space = TopologySpace.FromProject(project);
            _history.Record(Space);
            RefreshCommands();
            Space.Points.Clear();
            Space.OpenSets.Clear();
            foreach (var p in space.Points) Space.Points.Add(p);
            foreach (var o in space.OpenSets) Space.OpenSets.Add(o);
            UpdateMappings();
            ValidateTopology();
        }
    }

    private void ExportSpace()
    {
        var dlg = new SaveFileDialog
        {
            Filter = "JSON (*.json)|*.json",
            FileName = "topology.json",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
        };
        if (dlg.ShowDialog() == true)
        {
            var project = Space.ToProject("Построенное пространство");
            TopologyStorage.Save(dlg.FileName, project);
        }
    }

    private void ImportTargetSpace()
    {
        var dlg = new OpenFileDialog
        {
            Filter = "JSON (*.json)|*.json|Все файлы|*.*",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
        };
        if (dlg.ShowDialog() == true)
        {
            var project = TopologyStorage.Load(dlg.FileName);
            TargetSpace = TopologySpace.FromProject(project);
            TargetSpace.Points.CollectionChanged += (_, _) => UpdateMappings();
            UpdateMappings();
            OnPropertyChanged(nameof(TargetSpace));
        }
    }

    private void GenerateExample(string? key)
    {
        _history.Record(Space);
        RefreshCommands();
        Space.Points.Clear();
        Space.OpenSets.Clear();

        key ??= "discrete";
        TopologySpace example = key switch
        {
            "indiscrete" => TopologyExamples.CreateIndiscrete(3),
            "sierpinski" => TopologyExamples.CreateSierpinski(),
            "random" => TopologyExamples.CreateRandom(4),
            _ => TopologyExamples.CreateDiscrete(3)
        };

        foreach (var p in example.Points) Space.Points.Add(p);
        foreach (var o in example.OpenSets) Space.OpenSets.Add(o);
        SelectedPoints.Clear();
        UpdateMappings();
        ValidateTopology();
    }

    private void GenerateTargetSpace(string? key)
    {
        key ??= "sierpinski";
        TargetSpace = key switch
        {
            "discrete" => TopologyExamples.CreateDiscrete(3),
            "indiscrete" => TopologyExamples.CreateIndiscrete(3),
            "random" => TopologyExamples.CreateRandom(3),
            _ => TopologyExamples.CreateSierpinski()
        };
        TargetSpace.Points.CollectionChanged += (_, _) => UpdateMappings();
        UpdateMappings();
        OnPropertyChanged(nameof(TargetSpace));
    }

    private void CheckContinuity()
    {
        var mapping = PointMappings.ToDictionary(m => m.SourceId, m => m.TargetId);
        var result = Space.CheckContinuity(TargetSpace, mapping);
        ContinuityMessage = result.IsContinuous
            ? "Отображение непрерывно."
            : "Непрерывность нарушена: " + string.Join("; ", result.Issues);
    }

    private void OnPointsChanged()
    {
        foreach (var id in SelectedPoints.ToList())
        {
            if (Space.Points.All(p => p.Id != id))
                SelectedPoints.Remove(id);
        }
        UpdateMappings();
        ValidateTopology();
    }

    private void UpdateMappings()
    {
        PointMappings.Clear();
        foreach (var p in Space.Points)
        {
            var targetId = TargetSpace.Points.FirstOrDefault()?.Id ?? 0;
            PointMappings.Add(new PointMappingViewModel
            {
                SourceId = p.Id,
                SourceName = p.Name,
                TargetId = targetId
            });
        }
        ContinuityMessage = "Настройте соответствия и нажмите Проверить.";
        OnPropertyChanged(nameof(PointMappings));
        OnPropertyChanged(nameof(TargetSpace));
    }

    private void RefreshCommands() => System.Windows.Input.CommandManager.InvalidateRequerySuggested();

    public string DescribeOpenSet(OpenSet set)
    {
        var names = Space.Points
            .Where(p => (set.Mask & (1 << p.Id)) != 0)
            .Select(p => p.Name);
        return string.Join(", ", names);
    }

    public string GetTooltip(TopologyPoint point)
    {
        var minMask = Space.ComputeMinimalNeighborhood(point);
        var minNames = Space.Points.Where(p => (minMask & (1 << p.Id)) != 0).Select(p => p.Name);
        var containing = Space.OpenSets.Where(o => (o.Mask & (1 << point.Id)) != 0).Select(o => o.Name);
        return $"Минимальная окрестность: {{{string.Join(", ", minNames)}}}\nОткрытые множества: {string.Join(", ", containing)}";
    }

    private string GenerateColor()
    {
        var rnd = new Random();
        return $"#{rnd.Next(80, 200):X2}{rnd.Next(80, 200):X2}{rnd.Next(80, 200):X2}";
    }
}
