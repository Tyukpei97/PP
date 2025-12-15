using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Media;

namespace InteractiveBuildingCrowdSimulator.App.Models;

public enum ScenarioType
{
    Normal,
    Evacuation,
    Overcrowding
}

public enum AgentState
{
    Idle,
    Moving,
    Exited,
    Blocked
}

public abstract class Area
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "Область";
    public Rect Bounds { get; set; }

    [JsonIgnore]
    public Point Center => new(Bounds.X + Bounds.Width / 2, Bounds.Y + Bounds.Height / 2);

    public virtual bool Contains(Point point) => Bounds.Contains(point);
}

public class Room : Area
{
    public int Capacity { get; set; }
}

public class Corridor : Area
{
    public double PassageWidth { get; set; } = 2;
}

public class Stair : Area
{
    public int Levels { get; set; } = 2;
    public double PassageWidth { get; set; } = 2;
}

public class Obstacle
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Rect Bounds { get; set; }
}

public class Door
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid FromAreaId { get; set; }
    public Guid ToAreaId { get; set; }
    public double Width { get; set; } = 1;
    public double ThroughputPerSecond { get; set; } = 2;
    public bool OneWay { get; set; }
}

public class BuildingMap
{
    public ObservableCollection<Room> Rooms { get; set; } = new();
    public ObservableCollection<Corridor> Corridors { get; set; } = new();
    public ObservableCollection<Stair> Stairs { get; set; } = new();
    public ObservableCollection<Door> Doors { get; set; } = new();
    public ObservableCollection<Obstacle> Obstacles { get; set; } = new();

    [JsonIgnore]
    public IEnumerable<Area> AllAreas => Rooms.Cast<Area>()
        .Concat(Corridors)
        .Concat(Stairs);

    public Area? FindArea(Guid id) => AllAreas.FirstOrDefault(a => a.Id == id);

    public bool IsAccessible(Point point) => !Obstacles.Any(o => o.Bounds.Contains(point));
}

public class Agent
{
    public int Id { get; set; }
    public Guid CurrentAreaId { get; set; }
    public Guid GoalAreaId { get; set; }
    public double Speed { get; set; }
    public double Stress { get; set; }
    public AgentState State { get; set; } = AgentState.Idle;
    public Point Position { get; set; }
    public Queue<Point> CurrentPath { get; set; } = new();
}

public class ScenarioSettings
{
    public ScenarioType Type { get; set; } = ScenarioType.Normal;
    public int AgentCount { get; set; } = 50;
    public double MinSpeed { get; set; } = 0.8;
    public double MaxSpeed { get; set; } = 1.6;
    public double MinStress { get; set; } = 0.1;
    public double MaxStress { get; set; } = 0.8;
    public Guid? TargetAreaId { get; set; }
}

public class StatisticsSnapshot
{
    public TimeSpan SimulationTime { get; set; }
    public int AgentsRemaining { get; set; }
    public double AverageSpeed { get; set; }
    public double MaxDensity { get; set; }
    public IReadOnlyList<string> ProblemAreas { get; set; } = Array.Empty<string>();
}

public class AgentRenderState
{
    public int Id { get; init; }
    public Point Position { get; init; }
    public AgentState State { get; init; }
    public Color Color { get; init; }
}
