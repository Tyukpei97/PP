namespace Battleship.Core.Models;

public record PlayerStatistics(int Shots, int Hits, int Sunk, int Misses)
{
    public double Accuracy => Shots == 0 ? 0 : (double)Hits / Shots * 100;
}
