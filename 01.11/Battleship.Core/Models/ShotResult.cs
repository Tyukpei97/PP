using System.Collections.Generic;

namespace Battleship.Core.Models;

public class ShotResult
{
    public ShotOutcome Outcome { get; private set; }
    public Coordinate Coordinate { get; }
    public Ship? SunkShip { get; }
    public string? Error { get; }

    private ShotResult(ShotOutcome outcome, Coordinate coordinate, Ship? sunkShip = null, string? error = null)
    {
        Outcome = outcome;
        Coordinate = coordinate;
        SunkShip = sunkShip;
        Error = error;
    }

    public static ShotResult Invalid(string error, Coordinate coordinate) => new(ShotOutcome.Invalid, coordinate, null, error);
    public static ShotResult Repeat(Coordinate coordinate) => new(ShotOutcome.Repeat, coordinate);
    public static ShotResult Miss(Coordinate coordinate) => new(ShotOutcome.Miss, coordinate);
    public static ShotResult Hit(Coordinate coordinate) => new(ShotOutcome.Hit, coordinate);
    public static ShotResult Sunk(Coordinate coordinate, Ship ship) => new(ShotOutcome.Sunk, coordinate, ship);

    public IEnumerable<Coordinate> GetSunkDecks()
    {
        if (SunkShip == null)
        {
            yield break;
        }

        foreach (var deck in SunkShip.Decks)
        {
            yield return deck;
        }
    }
}
