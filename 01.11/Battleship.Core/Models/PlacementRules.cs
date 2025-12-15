using System.Collections.Generic;
using System.Linq;

namespace Battleship.Core.Models;

public static class PlacementRules
{
    private static readonly Dictionary<int, int> RequiredFleet = new()
    {
        {4, 1},
        {3, 2},
        {2, 3},
        {1, 4}
    };

    public static bool IsFleetComplete(PlayerState player, out string? error)
    {
        foreach (var requirement in RequiredFleet)
        {
            player.FleetCount.TryGetValue(requirement.Key, out var current);
            if (current != requirement.Value)
            {
                error = $"Нужно кораблей длины {requirement.Key}: {requirement.Value}";
                return false;
            }
        }

        error = null;
        return true;
    }

    public static void RegisterShip(PlayerState player, int length)
    {
        if (!player.FleetCount.ContainsKey(length))
        {
            player.FleetCount[length] = 0;
        }

        player.FleetCount[length]++;
    }

    public static void ResetFleet(PlayerState player)
    {
        player.FleetCount[1] = 0;
        player.FleetCount[2] = 0;
        player.FleetCount[3] = 0;
        player.FleetCount[4] = 0;
        player.Board.Reset();
        player.PlacementReady = false;
    }
}
