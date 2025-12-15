using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using InteractiveBuildingCrowdSimulator.App.Infrastructure;
using InteractiveBuildingCrowdSimulator.App.Models;
using InteractiveBuildingCrowdSimulator.App.Services;

namespace InteractiveBuildingCrowdSimulator.App.ViewModels;

/// <summary>
/// Управление запуском/остановкой симуляции и визуализация агентов.
/// </summary>
public class SimulationViewModel : ObservableObject
{
    private readonly SimulationEngine _engine;
    private readonly StatisticsService _statistics;
    private readonly AnalyticsViewModel _analyticsViewModel;
    private readonly BuildingMap _map;
    private readonly System.Windows.Threading.Dispatcher _dispatcher;
    private string _status = "Симуляция готова";
    private bool _isBusy;
    private AgentRenderState? _selectedAgent;

    public SimulationViewModel(SimulationEngine engine, StatisticsService statistics, AnalyticsViewModel analyticsViewModel, BuildingMap map)
    {
        _engine = engine;
        _statistics = statistics;
        _analyticsViewModel = analyticsViewModel;
        _map = map;
        _dispatcher = Application.Current.Dispatcher;

        _engine.Updated += OnEngineUpdated;
        _engine.Error += message => Status = message;

        StartCommand = new RelayCommand(_ => Start(), _ => !_engine.IsRunning);
        PauseCommand = new RelayCommand(_ => Pause(), _ => _engine.IsRunning);
        ResetCommand = new RelayCommand(_ => Reset());

        SelectedScenario = Scenarios.First();
    }

    public ObservableCollection<AgentRenderState> Agents { get; } = new();
    public BuildingMap Map => _map;

    public ScenarioSettings Settings { get; } = new();

    public ObservableCollection<ScenarioOption> Scenarios { get; } = new()
    {
        new(ScenarioType.Normal, "Обычный режим"),
        new(ScenarioType.Evacuation, "Эвакуация"),
        new(ScenarioType.Overcrowding, "Переполнение")
    };

    private ScenarioOption? _selectedScenario;

    public ScenarioOption? SelectedScenario
    {
        get => _selectedScenario;
        set
        {
            if (SetProperty(ref _selectedScenario, value) && value != null)
            {
                Settings.Type = value.Type;
            }
        }
    }

    public RelayCommand StartCommand { get; }
    public RelayCommand PauseCommand { get; }
    public RelayCommand ResetCommand { get; }

    public string Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    public AgentRenderState? SelectedAgent
    {
        get => _selectedAgent;
        set => SetProperty(ref _selectedAgent, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    private void Start()
    {
        IsBusy = true;
        _statistics.Clear();
        Settings.AgentCount = Math.Clamp(Settings.AgentCount, 1, 2000);
        _engine.Configure(_map, Settings);
        _engine.Start();
        Status = "Симуляция запущена";
        IsBusy = false;
        StartCommand.RaiseCanExecuteChanged();
        PauseCommand.RaiseCanExecuteChanged();
    }

    private void Pause()
    {
        _engine.Pause();
        Status = "Симуляция на паузе";
        StartCommand.RaiseCanExecuteChanged();
        PauseCommand.RaiseCanExecuteChanged();
    }

    private void Reset()
    {
        _engine.Pause();
        Agents.Clear();
        _statistics.Clear();
        Status = "Сброшено";
        StartCommand.RaiseCanExecuteChanged();
        PauseCommand.RaiseCanExecuteChanged();
    }

    private void OnEngineUpdated(IReadOnlyList<Agent> agents, TimeSpan time)
    {
        _ = _dispatcher.InvokeAsync(() =>
        {
            Agents.Clear();
            foreach (var agent in agents)
            {
                Agents.Add(ToRenderState(agent));
            }

            if (SelectedAgent is not null)
            {
                SelectedAgent = Agents.FirstOrDefault(a => a.Id == SelectedAgent.Id);
            }

            var snapshot = _statistics.Capture(_map, agents.ToList(), time);
            _analyticsViewModel.AddSnapshot(snapshot);
            Status = $"Время: {time:mm\\:ss}; Осталось: {snapshot.AgentsRemaining}";
        });
    }

    public void SelectAgentByPosition(Point point, double radius)
    {
        var found = Agents.FirstOrDefault(a => (a.Position - point).Length <= radius);
        SelectedAgent = found;
    }

    private static AgentRenderState ToRenderState(Agent agent)
    {
        var color = agent.State switch
        {
            AgentState.Exited => Colors.LightGreen,
            AgentState.Blocked => Colors.OrangeRed,
            _ => Colors.SteelBlue
        };

        return new AgentRenderState
        {
            Id = agent.Id,
            Position = agent.Position,
            State = agent.State,
            Color = color
        };
    }
}

public record ScenarioOption(ScenarioType Type, string Title);
