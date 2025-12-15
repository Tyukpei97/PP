using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Battleship.Core.Models;

namespace Battleship.Core.Persistence;

public static class GameStorage
{
    private const string FolderName = "Battleship\\Saves";

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true
    };

    public static string EnsureSaveDirectory()
    {
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), FolderName);
        Directory.CreateDirectory(path);
        return path;
    }

    public static async Task<string> SaveAsync(GameSession session, string? fileName = null)
    {
        var dir = EnsureSaveDirectory();
        var name = fileName ?? $"save_{DateTime.Now:yyyyMMdd_HHmmss}.json";
        var path = Path.Combine(dir, name);

        var dto = GameSessionDto.FromSession(session);
        await using var fs = File.Create(path);
        await JsonSerializer.SerializeAsync(fs, dto, Options);
        return path;
    }

    public static async Task<GameSession?> LoadAsync(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        await using var fs = File.OpenRead(path);
        var dto = await JsonSerializer.DeserializeAsync<GameSessionDto>(fs, Options);
        return dto?.ToSession();
    }
}

public record CoordinateDto(int X, int Y);
public record ShipDto(List<CoordinateDto> Decks);

public record PlayerStateDto(
    string Nickname,
    bool PlacementReady,
    int Shots,
    int Hits,
    int Sunk,
    int Misses,
    List<ShipDto> Ships,
    List<CoordinateDto> HitCells,
    List<CoordinateDto> MissCells);

public record GameSessionDto(
    Guid SessionId,
    GamePhase Phase,
    PlayerRole Turn,
    int TurnSecondsLeft,
    PlayerStateDto Server,
    PlayerStateDto Client)
{
    public static GameSessionDto FromSession(GameSession session)
    {
        return new GameSessionDto(
            session.SessionId,
            session.Phase,
            session.Turn,
            session.TurnSecondsLeft,
            ToDto(session.Server),
            ToDto(session.Client));
    }

    private static PlayerStateDto ToDto(PlayerState state)
    {
        return new PlayerStateDto(
            state.Nickname,
            state.PlacementReady,
            state.Shots,
            state.Hits,
            state.Sunk,
            state.Misses,
            state.Board.Ships.Select(s => new ShipDto(s.Decks.Select(d => new CoordinateDto(d.X, d.Y)).ToList())).ToList(),
            state.Board.Hits.Select(h => new CoordinateDto(h.X, h.Y)).ToList(),
            state.Board.Misses.Select(m => new CoordinateDto(m.X, m.Y)).ToList());
    }

    public GameSession ToSession()
    {
        var session = new GameSession
        {
            SessionId = SessionId,
            TurnSecondsLeft = TurnSecondsLeft
        };

        ApplyState(session.Server, Server);
        ApplyState(session.Client, Client);

        session.Turn = Turn;
        session.LastActionUtc = DateTime.UtcNow;
        session.Phase = Phase;
        return session;
    }

    private static void ApplyState(PlayerState target, PlayerStateDto dto)
    {
        target.Nickname = dto.Nickname;
        target.PlacementReady = dto.PlacementReady;
        target.Shots = dto.Shots;
        target.Hits = dto.Hits;
        target.Sunk = dto.Sunk;
        target.Misses = dto.Misses;
        PlacementRules.ResetFleet(target);

        foreach (var ship in dto.Ships)
        {
            var coords = ship.Decks.Select(d => new Coordinate(d.X, d.Y)).ToList();
            target.Board.RestoreShip(coords);
            PlacementRules.RegisterShip(target, coords.Count);
        }

        foreach (var hit in dto.HitCells)
        {
            target.Board.MarkHit(new Coordinate(hit.X, hit.Y));
        }

        foreach (var miss in dto.MissCells)
        {
            target.Board.MarkMiss(new Coordinate(miss.X, miss.Y));
        }
    }
}
