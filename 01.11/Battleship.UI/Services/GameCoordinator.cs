using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Battleship.Core.Engine;
using Battleship.Core.Models;
using Battleship.Core.Networking;
using Battleship.Core.Persistence;

namespace Battleship.UI.Services;

public class GameCoordinator : IDisposable
{
    private TcpServerHost? _serverHost;
    private TcpClientConnection? _client;
    private GameEngine? _engine;

    public bool IsHost { get; private set; }
    public GameStateSnapshot? Snapshot { get; private set; }
    public Guid SessionId => Snapshot?.SessionId ?? Guid.Empty;
    public string LastSavePath { get; private set; } = string.Empty;
    public string? LastHost { get; private set; }
    public int LastPort { get; private set; }
    public string CurrentNickname { get; private set; } = string.Empty;

    public event Action<GameStateSnapshot>? SnapshotChanged;
    public event Action<string>? Status;
    public event Action<string>? Error;
    public event Action<int>? Timer;

    public async Task StartHostAsync(int port, string nickname)
    {
        IsHost = true;
        CurrentNickname = nickname;
        Stop();
        _engine = new GameEngine();
        _engine.Session.Server.Nickname = nickname;
        _serverHost = new TcpServerHost(_engine);
        _serverHost.Status += msg => Status?.Invoke(msg);
        _serverHost.Error += msg => Error?.Invoke(msg);
        _serverHost.SnapshotUpdated += snapshot =>
        {
            Snapshot = snapshot;
            SnapshotChanged?.Invoke(snapshot);
        };
        await _serverHost.StartAsync(port);
    }

    public async Task StartClientAsync(string host, int port, string nickname)
    {
        IsHost = false;
        Stop();
        LastHost = host;
        LastPort = port;
        CurrentNickname = nickname;
        _client = new TcpClientConnection();
        _client.Status += msg => Status?.Invoke(msg);
        _client.Error += msg => Error?.Invoke(msg);
        _client.Timer += sec => Timer?.Invoke(sec);
        _client.SnapshotReceived += snapshot =>
        {
            Snapshot = snapshot;
            SnapshotChanged?.Invoke(snapshot);
        };
        await _client.ConnectAsync(host, port, nickname);
    }

    public async Task<bool> ReconnectAsync(string host, int port, Guid sessionId)
    {
        if (_client == null)
        {
            _client = new TcpClientConnection();
            _client.Status += msg => Status?.Invoke(msg);
            _client.Error += msg => Error?.Invoke(msg);
            _client.Timer += sec => Timer?.Invoke(sec);
            _client.SnapshotReceived += snapshot =>
            {
                Snapshot = snapshot;
                SnapshotChanged?.Invoke(snapshot);
            };
        }

        LastHost = host;
        LastPort = port;
        await _client.ReconnectAsync(host, port, sessionId);
        return true;
    }

    public void ApplyPlacement(IEnumerable<PlacedShip> ships)
    {
        if (IsHost)
        {
            if (_serverHost == null || _engine == null) return;
            _engine.ResetPlacement(PlayerRole.Server);
            foreach (var ship in ships)
            {
                _serverHost.PlaceShipForHost(ship.Start, ship.Length, ship.Orientation);
            }
            _serverHost.CompletePlacementForHost();
        }
        else
        {
            _ = _client?.SendPlacementAsync(ships);
        }
    }

    public void Shoot(Coordinate coordinate)
    {
        if (IsHost)
        {
            _serverHost?.ShootForHost(coordinate);
        }
        else
        {
            _ = _client?.SendShotAsync(coordinate);
        }
    }

    public async Task<bool> SaveAsync()
    {
        if (!IsHost || _serverHost == null) return false;
        LastSavePath = await _serverHost.SaveAsync();
        Status?.Invoke($"Сохранено: {LastSavePath}");
        return true;
    }

    public async Task<bool> LoadAsync(string path)
    {
        if (!IsHost || _serverHost == null) return false;
        var success = await _serverHost.LoadAsync(path);
        if (success)
        {
            Status?.Invoke($"Загружено: {path}");
        }
        return success;
    }

    public IEnumerable<string> GetSaveFiles()
    {
        var dir = GameStorage.EnsureSaveDirectory();
        if (!Directory.Exists(dir))
        {
            return Enumerable.Empty<string>();
        }

        return Directory.GetFiles(dir, "*.json");
    }

    public void Dispose()
    {
        _serverHost?.Dispose();
        _client?.Dispose();
        _engine?.Dispose();
    }

    public void Stop()
    {
        _serverHost?.Dispose();
        _client?.Dispose();
        _engine?.Dispose();
        _serverHost = null;
        _client = null;
        _engine = null;
        Snapshot = null;
    }
}
