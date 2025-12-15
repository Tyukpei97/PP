using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Battleship.Core.Models;

public record BoardSnapshot(int Size, CellState[][] Cells)
{
    public static BoardSnapshot ForOwner(Board board)
    {
        var cells = new CellState[Board.Size][];
        for (var y = 0; y < Board.Size; y++)
        {
            cells[y] = new CellState[Board.Size];
            for (var x = 0; x < Board.Size; x++)
            {
                cells[y][x] = board.GetOwnerCellState(new Coordinate(x, y));
            }
        }

        return new BoardSnapshot(Board.Size, cells);
    }

    public static BoardSnapshot ForOpponent(Board board)
    {
        var cells = new CellState[Board.Size][];
        for (var y = 0; y < Board.Size; y++)
        {
            cells[y] = new CellState[Board.Size];
            for (var x = 0; x < Board.Size; x++)
            {
                cells[y][x] = board.GetOpponentCellState(new Coordinate(x, y));
            }
        }

        return new BoardSnapshot(Board.Size, cells);
    }
}

public record PlayerSnapshot(
    string Nickname,
    PlayerStatistics Statistics,
    bool Ready,
    int ShipsAlive);

public record GameStateSnapshot(
    Guid SessionId,
    GamePhase Phase,
    PlayerRole Viewer,
    PlayerRole Turn,
    BoardSnapshot OwnBoard,
    BoardSnapshot OpponentBoard,
    PlayerSnapshot You,
    PlayerSnapshot Opponent,
    string Status,
    int TurnSecondsLeft);
