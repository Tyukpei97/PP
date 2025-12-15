using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using InteractiveBuildingCrowdSimulator.App.Models;

namespace InteractiveBuildingCrowdSimulator.App.Services;

/// <summary>
/// Простая реализация A* по графу областей здания.
/// </summary>
public class PathfindingService
{
    public IReadOnlyList<Point> FindPath(BuildingMap map, Guid startAreaId, Guid goalAreaId)
    {
        var start = map.FindArea(startAreaId);
        var goal = map.FindArea(goalAreaId);
        if (start is null || goal is null)
        {
            return Array.Empty<Point>();
        }

        var open = new PriorityQueue<Guid, double>();
        var cameFrom = new Dictionary<Guid, Guid>();
        var gScore = new Dictionary<Guid, double> { [startAreaId] = 0 };

        open.Enqueue(startAreaId, 0);

        while (open.TryDequeue(out var current, out _))
        {
            if (current == goalAreaId)
            {
                return Reconstruct(map, cameFrom, current, startAreaId, goalAreaId);
            }

            foreach (var neighbor in GetNeighbors(map, current))
            {
                var currentArea = map.FindArea(current);
                var neighborArea = map.FindArea(neighbor.areaId);
                if (currentArea is null || neighborArea is null)
                {
                    continue;
                }

                var distance = Distance(currentArea.Center, neighborArea.Center) / Math.Max(neighbor.door.Width, 0.4);
                var tentativeScore = gScore[current] + distance;
                if (!gScore.TryGetValue(neighbor.areaId, out var existing) || tentativeScore < existing)
                {
                    cameFrom[neighbor.areaId] = current;
                    gScore[neighbor.areaId] = tentativeScore;
                    var priority = tentativeScore + Distance(neighborArea.Center, goal.Center);
                    open.Enqueue(neighbor.areaId, priority);
                }
            }
        }

        return Array.Empty<Point>();
    }

    private static double Distance(Point a, Point b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    private static IReadOnlyList<Point> Reconstruct(BuildingMap map, Dictionary<Guid, Guid> cameFrom, Guid current, Guid start, Guid goal)
    {
        var totalPath = new List<Guid> { current };
        while (cameFrom.TryGetValue(current, out var prev))
        {
            current = prev;
            totalPath.Add(current);
        }

        totalPath.Reverse();

        var points = totalPath
            .Select(id => map.FindArea(id)?.Center ?? new Point())
            .ToList();

        if (!points.Any() || points.First() != map.FindArea(start)?.Center)
        {
            points.Insert(0, map.FindArea(start)?.Center ?? new Point());
        }

        if (points.Last() != map.FindArea(goal)?.Center)
        {
            points.Add(map.FindArea(goal)?.Center ?? new Point());
        }

        return points;
    }

    private static IEnumerable<(Guid areaId, Door door)> GetNeighbors(BuildingMap map, Guid areaId)
    {
        foreach (var door in map.Doors)
        {
            if (door.OneWay && door.FromAreaId != areaId)
            {
                continue;
            }

            if (door.FromAreaId == areaId)
            {
                yield return (door.ToAreaId, door);
            }
            else if (!door.OneWay && door.ToAreaId == areaId)
            {
                yield return (door.FromAreaId, door);
            }
        }
    }
}
