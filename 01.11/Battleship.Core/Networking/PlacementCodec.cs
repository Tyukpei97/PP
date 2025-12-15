using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Battleship.Core.Models;

namespace Battleship.Core.Networking;

public static class PlacementCodec
{
    public static string Encode(IEnumerable<PlacedShip> ships)
    {
        return string.Join(";", ships.Select(s => $"{s.Start.X},{s.Start.Y},{s.Length},{(s.Orientation == Orientation.Horizontal ? "H" : "V")}"));
    }

    public static bool TryDecode(string payload, out List<PlacedShip> ships)
    {
        ships = new List<PlacedShip>();
        if (string.IsNullOrWhiteSpace(payload))
        {
            return false;
        }

        var parts = payload.Split(';', StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in parts)
        {
            var items = part.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (items.Length != 4)
            {
                ships.Clear();
                return false;
            }

            if (!int.TryParse(items[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var x) ||
                !int.TryParse(items[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var y) ||
                !int.TryParse(items[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var len))
            {
                ships.Clear();
                return false;
            }

            var orientation = items[3].ToUpperInvariant() == "H" ? Orientation.Horizontal : Orientation.Vertical;
            ships.Add(new PlacedShip(new Coordinate(x, y), len, orientation));
        }

        return ships.Count > 0;
    }
}
