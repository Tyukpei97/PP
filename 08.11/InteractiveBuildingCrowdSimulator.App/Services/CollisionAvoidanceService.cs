using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using InteractiveBuildingCrowdSimulator.App.Models;

namespace InteractiveBuildingCrowdSimulator.App.Services;

/// <summary>
/// Оценивает локальную плотность и корректирует скорость агентов.
/// </summary>
public class CollisionAvoidanceService
{
    private readonly double _influenceRadius;

    public CollisionAvoidanceService(double influenceRadius = 2.0)
    {
        _influenceRadius = influenceRadius;
    }

    public double GetSpeedFactor(Agent agent, IReadOnlyList<Agent> agents)
    {
        var neighbors = agents.Where(a => a.Id != agent.Id)
            .Select(a => (agent.Position - a.Position).Length)
            .Where(d => d < _influenceRadius)
            .ToList();

        if (neighbors.Count == 0)
        {
            return 1.0;
        }

        var density = neighbors.Count / (_influenceRadius * _influenceRadius * Math.PI);
        var factor = 1.0 / (1.0 + density); // замедление при росте плотности
        return Math.Clamp(factor, 0.25, 1.0);
    }

    public double AdjustForDoor(Agent agent, Door door)
    {
        if (door.ThroughputPerSecond <= 0.1)
        {
            return 0.3;
        }

        var capacityFactor = Math.Clamp(door.ThroughputPerSecond / 3.0, 0.4, 1.0);
        var widthFactor = Math.Clamp(door.Width / 2.0, 0.3, 1.0);
        return Math.Min(capacityFactor, widthFactor);
    }
}
