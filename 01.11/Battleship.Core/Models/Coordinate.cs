namespace Battleship.Core.Models;

public readonly record struct Coordinate(int X, int Y)
{
    public bool IsInside(int size) => X >= 0 && Y >= 0 && X < size && Y < size;
}
