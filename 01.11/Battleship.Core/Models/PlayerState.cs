using System.Collections.Generic;

namespace Battleship.Core.Models;

public class PlayerState
{
    public string Nickname { get; set; } = string.Empty;
    public Board Board { get; } = new();
    public bool PlacementReady { get; set; }

    public int Shots { get; set; }
    public int Hits { get; set; }
    public int Sunk { get; set; }
    public int Misses { get; set; }

    public PlayerStatistics Stats => new(Shots, Hits, Sunk, Misses);

    public Dictionary<int, int> FleetCount { get; } = new()
    {
        {4, 0},
        {3, 0},
        {2, 0},
        {1, 0}
    };
}
