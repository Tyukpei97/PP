using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using Battleship.Core.Models;

namespace Battleship.Core.Networking;

public record ProtocolMessage(string Type, string[] Parts);

public static class Protocol
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public static bool TryParse(string line, out ProtocolMessage message)
    {
        var parts = line.Split('|', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            message = new ProtocolMessage(string.Empty, Array.Empty<string>());
            return false;
        }

        message = new ProtocolMessage(parts[0].Trim(), parts.Skip(1).ToArray());
        return true;
    }

    public static string Hello(string nickname) => Build("HELLO", nickname);
    public static string Role(PlayerRole role) => Build("ROLE", role.ToString().ToUpperInvariant());
    public static string Phase(GamePhase phase) => Build("PHASE", phase.ToString().ToUpperInvariant());
    public static string Turn(PlayerRole turn) => Build("TURN", turn == PlayerRole.Server ? "YOU" : "OPPONENT");
    public static string TurnForClient(PlayerRole turn) => Build("TURN", turn == PlayerRole.Client ? "YOU" : "OPPONENT");
    public static string Shot(int x, int y) => Build("SHOT", x.ToString(), y.ToString());
    public static string ShotResult(ShotOutcome outcome, int x, int y) => Build("SHOT_RESULT", x.ToString(), y.ToString(), outcome.ToString().ToUpperInvariant());
    public static string Timer(int seconds) => Build("TIMER", seconds.ToString());
    public static string Error(string message) => Build("ERROR", message);
    public static string Reconnect(Guid sessionId) => Build("RECONNECT", sessionId.ToString());

    public static string State(GameStateSnapshot snapshot)
    {
        var json = JsonSerializer.Serialize(snapshot, JsonOptions);
        var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        return Build("STATE", encoded);
    }

    public static bool TryDecodeState(string payload, out GameStateSnapshot? snapshot)
    {
        try
        {
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
            snapshot = JsonSerializer.Deserialize<GameStateSnapshot>(json, JsonOptions);
            return snapshot != null;
        }
        catch
        {
            snapshot = null;
            return false;
        }
    }

    public static string Build(string type, params string[] parts)
    {
        return string.Join("|", new[] { type }.Concat(parts.Select(Escape)));
    }

    private static string Escape(string value) => value.Replace("\r", " ").Replace("\n", " ").Trim();
}
