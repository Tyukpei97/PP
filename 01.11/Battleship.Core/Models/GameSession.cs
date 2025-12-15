using System;
using System.Linq;

namespace Battleship.Core.Models;

public class GameSession
{
    private readonly Random _random = new();

    public Guid SessionId { get; init; } = Guid.NewGuid();
    public PlayerState Server { get; } = new();
    public PlayerState Client { get; } = new();

    public GamePhase Phase { get; internal set; } = GamePhase.Lobby;
    public PlayerRole Turn { get; internal set; } = PlayerRole.Server;
    public DateTime LastActionUtc { get; internal set; } = DateTime.UtcNow;

    public TimeSpan TurnDuration { get; set; } = TimeSpan.FromSeconds(30);
    public int TurnSecondsLeft { get; internal set; } = 30;

    public void SetNickname(PlayerRole role, string nickname)
    {
        GetPlayer(role).Nickname = nickname;
    }

    public (bool ok, string? error) TryPlaceShip(PlayerRole role, Coordinate start, int length, Orientation orientation)
    {
        var player = GetPlayer(role);
        if (Phase != GamePhase.Placement && Phase != GamePhase.Lobby)
        {
            return (false, "Этап размещения завершён");
        }

        if (!player.Board.CanPlaceShip(start, length, orientation, out var reason))
        {
            return (false, reason);
        }

        player.Board.PlaceShip(start, length, orientation);
        PlacementRules.RegisterShip(player, length);
        Phase = GamePhase.Placement;
        LastActionUtc = DateTime.UtcNow;
        return (true, null);
    }

    public (bool ok, string? error) TryCompletePlacement(PlayerRole role)
    {
        var player = GetPlayer(role);
        if (!PlacementRules.IsFleetComplete(player, out var error))
        {
            return (false, error);
        }

        player.PlacementReady = true;
        Phase = GamePhase.Placement;

        if (Server.PlacementReady && Client.PlacementReady)
        {
            StartBattle();
        }

        LastActionUtc = DateTime.UtcNow;
        return (true, null);
    }

    private void StartBattle()
    {
        Phase = GamePhase.Battle;
        Turn = _random.Next(0, 2) == 0 ? PlayerRole.Server : PlayerRole.Client;
        ResetTurnTimer();
    }

    public (ShotResult? result, string? error) TryProcessShot(PlayerRole shooter, Coordinate coordinate)
    {
        if (Phase != GamePhase.Battle)
        {
            return (null, "Сейчас не этап боя");
        }

        if (Turn != shooter)
        {
            return (null, "Ход соперника");
        }

        var opponent = GetOpponent(shooter);
        var shotResult = opponent.Board.ApplyShot(coordinate);
        if (shotResult.Outcome == ShotOutcome.Invalid)
        {
            return (null, shotResult.Error ?? "Неверный выстрел");
        }

        if (shotResult.Outcome == ShotOutcome.Repeat)
        {
            return (null, "Сюда уже стреляли");
        }

        var shooterState = GetPlayer(shooter);
        shooterState.Shots++;

        switch (shotResult.Outcome)
        {
            case ShotOutcome.Miss:
                shooterState.Misses++;
                SwitchTurn();
                break;
            case ShotOutcome.Hit:
                shooterState.Hits++;
                break;
            case ShotOutcome.Sunk:
                shooterState.Hits++;
                shooterState.Sunk++;
                break;
        }

        if (opponent.Board.AllShipsSunk())
        {
            Phase = GamePhase.GameOver;
        }

        LastActionUtc = DateTime.UtcNow;
        ResetTurnTimer();
        return (shotResult, null);
    }

    public GameStateSnapshot CreateSnapshot(PlayerRole viewer, string status)
    {
        var you = GetPlayer(viewer);
        var opponent = GetOpponent(viewer);

        var ownBoard = BoardSnapshot.ForOwner(you.Board);
        var opponentBoard = BoardSnapshot.ForOpponent(opponent.Board);

        var youSnapshot = new PlayerSnapshot(
            you.Nickname,
            you.Stats,
            you.PlacementReady,
            CountAliveShips(you.Board));

        var opponentSnapshot = new PlayerSnapshot(
            opponent.Nickname,
            opponent.Stats,
            opponent.PlacementReady,
            CountAliveShips(opponent.Board));

        return new GameStateSnapshot(
            SessionId,
            Phase,
            viewer,
            Turn,
            ownBoard,
            opponentBoard,
            youSnapshot,
            opponentSnapshot,
            status,
            TurnSecondsLeft);
    }

    public void ResetTurnTimer()
    {
        TurnSecondsLeft = (int)TurnDuration.TotalSeconds;
    }

    public void TickTimer()
    {
        if (Phase != GamePhase.Battle)
        {
            return;
        }

        if (TurnSecondsLeft <= 0)
        {
            SwitchTurn();
            ResetTurnTimer();
            return;
        }

        TurnSecondsLeft--;
    }

    public void SwitchTurn()
    {
        Turn = Turn == PlayerRole.Server ? PlayerRole.Client : PlayerRole.Server;
    }

    public bool IsExpired(TimeSpan window) => DateTime.UtcNow - LastActionUtc > window;

    public PlayerState GetPlayer(PlayerRole role) => role == PlayerRole.Server ? Server : Client;
    public PlayerState GetOpponent(PlayerRole role) => role == PlayerRole.Server ? Client : Server;

    private static int CountAliveShips(Board board)
    {
        var hits = board.Hits.ToHashSet();
        return board.Ships.Count(s => !s.IsSunk(hits));
    }
}
