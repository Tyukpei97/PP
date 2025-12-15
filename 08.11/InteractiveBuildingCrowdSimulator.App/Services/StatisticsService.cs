using System;
using System.Collections.Generic;
using System.Linq;
using InteractiveBuildingCrowdSimulator.App.Models;

namespace InteractiveBuildingCrowdSimulator.App.Services;

/// <summary>
/// Подсчет метрик и истории для вкладки аналитики.
/// </summary>
public class StatisticsService
{
    private readonly List<StatisticsSnapshot> _history = new();

    public IReadOnlyList<StatisticsSnapshot> History => _history;

    public StatisticsSnapshot Capture(BuildingMap map, IReadOnlyList<Agent> agents, TimeSpan time)
    {
        var remaining = agents.Count(a => a.State != AgentState.Exited);
        var avgSpeed = agents.Any() ? agents.Average(a => a.Speed) : 0;

        var densities = map.AllAreas
            .Select(area =>
            {
                var inside = agents.Count(a => area.Contains(a.Position));
                var square = Math.Max(area.Bounds.Width * area.Bounds.Height, 1);
                var density = inside / square;
                return (area, inside, density);
            })
            .ToList();

        var maxDensity = densities.MaxBy(d => d.density).density;
        var problemAreas = densities
            .Where(d => d.density > 0.8)
            .Select(d => $"{d.area.Name}: плотность {d.density:0.00}")
            .Take(3)
            .ToList();

        var snapshot = new StatisticsSnapshot
        {
            SimulationTime = time,
            AgentsRemaining = remaining,
            AverageSpeed = avgSpeed,
            MaxDensity = maxDensity,
            ProblemAreas = problemAreas
        };

        _history.Add(snapshot);
        if (_history.Count > 600)
        {
            _history.RemoveAt(0);
        }

        return snapshot;
    }

    public void Clear() => _history.Clear();
}
