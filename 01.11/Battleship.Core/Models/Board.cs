using System;
using System.Collections.Generic;
using System.Linq;

namespace Battleship.Core.Models;

public class Board
{
    public const int Size = 10;

    private readonly List<Ship> _ships = new();
    private readonly HashSet<Coordinate> _hits = new();
    private readonly HashSet<Coordinate> _misses = new();

    public IReadOnlyList<Ship> Ships => _ships;
    public IReadOnlyCollection<Coordinate> Hits => _hits;
    public IReadOnlyCollection<Coordinate> Misses => _misses;

    public void Reset()
    {
        _ships.Clear();
        _hits.Clear();
        _misses.Clear();
    }

    public bool CanPlaceShip(Coordinate start, int length, Orientation orientation, out string? reason)
    {
        reason = null;
        var positions = CalculatePositions(start, length, orientation);
        if (positions.Any(p => !p.IsInside(Size)))
        {
            reason = "Корабль выходит за пределы поля";
            return false;
        }

        foreach (var pos in positions)
        {
            if (_ships.Any(s => s.Decks.Contains(pos)))
            {
                reason = "Корабли пересекаются";
                return false;
            }

            // запрещаем примыкание по соседним клеткам
            if (HasNeighbourShip(pos))
            {
                reason = "Корабли касаются друг друга";
                return false;
            }
        }

        return true;
    }

    public Ship PlaceShip(Coordinate start, int length, Orientation orientation)
    {
        if (!CanPlaceShip(start, length, orientation, out var reason))
        {
            throw new InvalidOperationException(reason);
        }

        var positions = CalculatePositions(start, length, orientation);
        var ship = new Ship(positions);
        _ships.Add(ship);
        return ship;
    }

    public ShotResult ApplyShot(Coordinate coordinate)
    {
        if (!coordinate.IsInside(Size))
        {
            return ShotResult.Invalid("Выстрел вне поля", coordinate);
        }

        if (_hits.Contains(coordinate) || _misses.Contains(coordinate))
        {
            return ShotResult.Repeat(coordinate);
        }

        var targetShip = _ships.FirstOrDefault(s => s.Decks.Contains(coordinate));
        if (targetShip == null)
        {
            _misses.Add(coordinate);
            return ShotResult.Miss(coordinate);
        }

        _hits.Add(coordinate);
        var sunk = targetShip.IsSunk(_hits);
        return sunk ? ShotResult.Sunk(coordinate, targetShip) : ShotResult.Hit(coordinate);
    }

    public bool AllShipsSunk() => _ships.Count > 0 && _ships.All(s => s.IsSunk(_hits));

    public void RestoreShip(IEnumerable<Coordinate> decks)
    {
        _ships.Add(new Ship(decks));
    }

    public void MarkHit(Coordinate coordinate) => _hits.Add(coordinate);
    public void MarkMiss(Coordinate coordinate) => _misses.Add(coordinate);

    public CellState GetOwnerCellState(Coordinate coordinate)
    {
        var hasShip = _ships.Any(s => s.Decks.Contains(coordinate));
        if (_hits.Contains(coordinate))
        {
            return hasShip ? CellState.Hit : CellState.Miss;
        }

        if (_misses.Contains(coordinate))
        {
            return CellState.Miss;
        }

        return hasShip ? CellState.Ship : CellState.Empty;
    }

    public CellState GetOpponentCellState(Coordinate coordinate)
    {
        if (_hits.Contains(coordinate))
        {
            var ship = _ships.FirstOrDefault(s => s.Decks.Contains(coordinate));
            var sunk = ship?.IsSunk(_hits) == true;
            return sunk ? CellState.Sunk : CellState.Hit;
        }

        if (_misses.Contains(coordinate))
        {
            return CellState.Miss;
        }

        return CellState.Unknown;
    }

    private static List<Coordinate> CalculatePositions(Coordinate start, int length, Orientation orientation)
    {
        var positions = new List<Coordinate>(length);
        for (var i = 0; i < length; i++)
        {
            positions.Add(orientation == Orientation.Horizontal
                ? new Coordinate(start.X + i, start.Y)
                : new Coordinate(start.X, start.Y + i));
        }

        return positions;
    }

    private bool HasNeighbourShip(Coordinate coordinate)
    {
        for (var dx = -1; dx <= 1; dx++)
        {
            for (var dy = -1; dy <= 1; dy++)
            {
                var c = new Coordinate(coordinate.X + dx, coordinate.Y + dy);
                if (!c.IsInside(Size))
                {
                    continue;
                }

                if (_ships.Any(s => s.Decks.Contains(c)))
                {
                    return true;
                }
            }
        }

        return false;
    }
}
