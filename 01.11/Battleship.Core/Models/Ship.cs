using System.Collections.Generic;
using System.Linq;

namespace Battleship.Core.Models;

public class Ship
{
    public int Length { get; }
    public List<Coordinate> Decks { get; }

    public Ship(IEnumerable<Coordinate> decks)
    {
        Decks = decks.ToList();
        Length = Decks.Count;
    }

    public bool IsSunk(HashSet<Coordinate> hits) => Decks.All(hits.Contains);
}
