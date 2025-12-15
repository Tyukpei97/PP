using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using InteractiveBuildingCrowdSimulator.App.Models;

namespace InteractiveBuildingCrowdSimulator.App.Services;

/// <summary>
/// Фоновый движок симуляции движения агентов.
/// </summary>
public class SimulationEngine
{
    private readonly PathfindingService _pathfindingService;
    private readonly CollisionAvoidanceService _avoidanceService;
    private readonly Random _random = new();
    private readonly List<Agent> _agents = new();

    private BuildingMap _map = new();
    private ScenarioSettings _settings = new();
    private CancellationTokenSource? _cts;
    private Task? _loop;
    private TimeSpan _simulationTime = TimeSpan.Zero;

    public SimulationEngine(PathfindingService pathfindingService, CollisionAvoidanceService avoidanceService)
    {
        _pathfindingService = pathfindingService;
        _avoidanceService = avoidanceService;
    }

    public IReadOnlyList<Agent> Agents => _agents;
    public TimeSpan SimulationTime => _simulationTime;
    public bool IsRunning => _cts is { IsCancellationRequested: false };

    public event Action<IReadOnlyList<Agent>, TimeSpan>? Updated;
    public event Action<string>? Error;

    public void Configure(BuildingMap map, ScenarioSettings settings)
    {
        _map = map;
        _settings = settings;
        _simulationTime = TimeSpan.Zero;
        _agents.Clear();
        SpawnAgents();
    }

    public void Start()
    {
        if (IsRunning)
        {
            return;
        }

        if (!_map.AllAreas.Any())
        {
            Error?.Invoke("Нет областей для запуска симуляции. Добавьте комнаты или коридоры.");
            return;
        }

        _cts = new CancellationTokenSource();
        _loop = Task.Run(() => RunLoop(_cts.Token));
    }

    public void Pause()
    {
        _cts?.Cancel();
    }

    private void SpawnAgents()
    {
        var areas = _map.AllAreas.ToList();
        if (areas.Count == 0)
        {
            return;
        }

        var targetArea = _settings.TargetAreaId != null
            ? _map.FindArea(_settings.TargetAreaId.Value) ?? areas.Last()
            : areas.Last();

        for (int i = 0; i < _settings.AgentCount; i++)
        {
            var startArea = areas[_random.Next(areas.Count)];
            var pos = RandomPointIn(startArea.Bounds);
            var speed = Lerp(_settings.MinSpeed, _settings.MaxSpeed, _random.NextDouble());
            var stress = Lerp(_settings.MinStress, _settings.MaxStress, _random.NextDouble());

            var agent = new Agent
            {
                Id = i + 1,
                CurrentAreaId = startArea.Id,
                GoalAreaId = targetArea.Id,
                Position = pos,
                Speed = speed,
                Stress = stress,
                State = AgentState.Moving
            };

            var pathPoints = _pathfindingService.FindPath(_map, startArea.Id, targetArea.Id);
            agent.CurrentPath = new Queue<Point>(pathPoints);

            _agents.Add(agent);
        }
    }

    private async Task RunLoop(CancellationToken token)
    {
        var sw = Stopwatch.StartNew();
        var lastUpdate = sw.Elapsed;
        var lastEmit = sw.Elapsed;

        while (!token.IsCancellationRequested)
        {
            var now = sw.Elapsed;
            var dt = now - lastUpdate;
            lastUpdate = now;
            _simulationTime += dt;

            StepAgents(dt.TotalSeconds);

            if ((now - lastEmit).TotalMilliseconds >= 33)
            {
                lastEmit = now;
                Updated?.Invoke(_agents.ToList(), _simulationTime);
            }

            try
            {
                await Task.Delay(10, token).ConfigureAwait(false);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
    }

    private void StepAgents(double dt)
    {
        foreach (var agent in _agents)
        {
            if (agent.State is AgentState.Exited or AgentState.Blocked)
            {
                continue;
            }

            UpdateArea(agent);

            if (!agent.CurrentPath.Any())
            {
                if (agent.CurrentAreaId == agent.GoalAreaId)
                {
                    agent.State = AgentState.Exited;
                }
                else
                {
                    var newPath = _pathfindingService.FindPath(_map, agent.CurrentAreaId, agent.GoalAreaId);
                    agent.CurrentPath = new Queue<Point>(newPath);
                    if (!agent.CurrentPath.Any())
                    {
                        agent.State = AgentState.Blocked;
                    }
                }

                continue;
            }

            var nextPoint = agent.CurrentPath.Peek();
            var direction = nextPoint - agent.Position;
            var distanceToNext = direction.Length;

            var scenarioFactor = _settings.Type switch
            {
                ScenarioType.Evacuation => 1.35,
                ScenarioType.Overcrowding => 0.9,
                _ => 1.0
            };

            var baseSpeed = agent.Speed * scenarioFactor * (1 + agent.Stress * 0.4);
            var avoidanceFactor = _avoidanceService.GetSpeedFactor(agent, _agents);
            var doorFactor = GetDoorFactor(agent);
            var step = baseSpeed * avoidanceFactor * doorFactor * dt;

            if (step >= distanceToNext || distanceToNext < 0.01)
            {
                agent.Position = nextPoint;
                agent.CurrentPath.Dequeue();
                if (!agent.CurrentPath.Any())
                {
                    agent.CurrentAreaId = agent.GoalAreaId;
                }
            }
            else
            {
                direction.Normalize();
                agent.Position = new Point(agent.Position.X + direction.X * step, agent.Position.Y + direction.Y * step);
            }
        }
    }

    private void UpdateArea(Agent agent)
    {
        var area = _map.AllAreas.FirstOrDefault(a => a.Contains(agent.Position));
        if (area is not null)
        {
            agent.CurrentAreaId = area.Id;
        }
    }

    private double GetDoorFactor(Agent agent)
    {
        double factor = 1.0;
        foreach (var door in _map.Doors)
        {
            var from = _map.FindArea(door.FromAreaId);
            var to = _map.FindArea(door.ToAreaId);
            if (from is null || to is null)
            {
                continue;
            }

            var mid = new Point((from.Center.X + to.Center.X) / 2, (from.Center.Y + to.Center.Y) / 2);
            var dist = (agent.Position - mid).Length;
            if (dist < 1.5)
            {
                factor = Math.Min(factor, _avoidanceService.AdjustForDoor(agent, door));
            }
        }

        return factor;
    }

    private Point RandomPointIn(Rect rect)
    {
        var x = rect.X + _random.NextDouble() * Math.Max(rect.Width, 1);
        var y = rect.Y + _random.NextDouble() * Math.Max(rect.Height, 1);
        return new Point(x, y);
    }

    private static double Lerp(double min, double max, double t) => min + (max - min) * t;
}
