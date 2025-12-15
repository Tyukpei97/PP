using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Battleship.Core.Engine;
using Battleship.Core.Models;
using Battleship.Core.Persistence;

namespace Battleship.Core.Networking;

public class TcpServerHost : IDisposable
{
    private readonly GameEngine _engine;
    private readonly SemaphoreSlim _sendLock = new(1, 1);
    private TcpListener? _listener;
    private TcpClient? _client;
    private StreamReader? _reader;
    private StreamWriter? _writer;
    private CancellationTokenSource? _cts;
    private Task? _acceptTask;
    private Task? _readTask;
    private DateTime _disconnectUtc;
    private bool _waitingReconnect;
    private readonly TimeSpan _reconnectWindow = TimeSpan.FromSeconds(90);

    public event Action<GameStateSnapshot>? SnapshotUpdated;
    public event Action<string>? Status;
    public event Action<string>? Error;

    public TcpServerHost(GameEngine engine)
    {
        _engine = engine;
        _engine.StateChanged += OnStateChanged;
        _engine.TimerTick += seconds => SendTimer(seconds);
    }

    public Guid SessionId => _engine.Session.SessionId;

    public async Task StartAsync(int port)
    {
        _cts = new CancellationTokenSource();
        _listener = new TcpListener(IPAddress.Any, port);
        _listener.Start();
        _acceptTask = Task.Run(() => AcceptLoopAsync(_cts.Token));
        Status?.Invoke($"Сервер запущен на порту {port}");
        SnapshotUpdated?.Invoke(_engine.Snapshot(PlayerRole.Server));
    }

    public async Task StopAsync()
    {
        _cts?.Cancel();
        _listener?.Stop();
        _client?.Close();
        _reader?.Dispose();
        _writer?.Dispose();
        if (_acceptTask != null) await _acceptTask;
        if (_readTask != null) await _readTask;
    }

    private async Task AcceptLoopAsync(CancellationToken token)
    {
        if (_listener == null) return;
        try
        {
            while (!token.IsCancellationRequested)
            {
                var tcpClient = await _listener.AcceptTcpClientAsync(token);
                await HandleNewClientAsync(tcpClient, token);
            }
        }
        catch (OperationCanceledException)
        {
            // ignore
        }
    }

    private async Task HandleNewClientAsync(TcpClient tcpClient, CancellationToken token)
    {
        _client = tcpClient;
        var stream = tcpClient.GetStream();
        _reader = new StreamReader(stream, Encoding.UTF8);
        _writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
        _waitingReconnect = false;

        Status?.Invoke("Клиент подключился");
        await SendAsync(Protocol.Role(PlayerRole.Server), token);
        await SendStateToClientAsync(token, "Подключение завершено");

        _readTask = Task.Run(() => ReadLoopAsync(token), token);
    }

    private async Task ReadLoopAsync(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested && _reader != null)
            {
                var line = await _reader.ReadLineAsync();
                if (line == null)
                {
                    HandleDisconnect();
                    return;
                }

                if (Protocol.TryParse(line, out var message))
                {
                    await HandleMessageAsync(message, token);
                }
            }
        }
        catch (IOException)
        {
            HandleDisconnect();
        }
        catch (Exception ex)
        {
            Error?.Invoke($"Ошибка сети: {ex.Message}");
            HandleDisconnect();
        }
    }

    private async Task HandleMessageAsync(ProtocolMessage message, CancellationToken token)
    {
        switch (message.Type.ToUpperInvariant())
        {
            case "HELLO":
                if (message.Parts.Length >= 1)
                {
                    _engine.Session.Client.Nickname = message.Parts[0];
                    Status?.Invoke($"Клиент: {_engine.Session.Client.Nickname}");
                    await SendAsync(Protocol.Hello("OK"), token);
                    await SendAsync(Protocol.Reconnect(_engine.Session.SessionId), token);
                    await SendStateToClientAsync(token, "Подключение установлено");
                }
                break;
            case "PLACE":
                if (message.Parts.Length >= 1 && PlacementCodec.TryDecode(message.Parts[0], out var ships))
                {
                    PlacementRules.ResetFleet(_engine.Session.Client);
                    foreach (var ship in ships)
                    {
                        _engine.PlaceShip(PlayerRole.Client, ship.Start, ship.Length, ship.Orientation);
                    }

                    _engine.CompletePlacement(PlayerRole.Client);
                    await SendStateToClientAsync(token, "Размещение принято");
                }
                else
                {
                    await SendAsync(Protocol.Error("Не удалось разобрать размещение"), token);
                }
                break;
            case "SHOT":
                if (message.Parts.Length >= 2 &&
                    int.TryParse(message.Parts[0], out var x) &&
                    int.TryParse(message.Parts[1], out var y))
                {
                    if (_engine.Shoot(PlayerRole.Client, new Coordinate(x, y)))
                    {
                        await SendStateToClientAsync(token, "Ход принят");
                    }
                    else
                    {
                        await SendAsync(Protocol.Error("Выстрел отклонён"), token);
                    }
                }
                break;
            case "RECONNECT":
                if (message.Parts.Length >= 1 && Guid.TryParse(message.Parts[0], out var sessionId))
                {
                    if (sessionId == _engine.Session.SessionId && !_engine.Session.IsExpired(_reconnectWindow))
                    {
                        Status?.Invoke("Клиент переподключился");
                        await SendStateToClientAsync(token, "Сессия восстановлена");
                    }
                    else
                    {
                        await SendAsync(Protocol.Error("Сессия не найдена или устарела"), token);
                    }
                }
                break;
        }
    }

    private void HandleDisconnect()
    {
        _disconnectUtc = DateTime.UtcNow;
        _waitingReconnect = true;
        _engine.Session.LastActionUtc = DateTime.UtcNow;
        Status?.Invoke("Клиент отключился. Ожидание переподключения.");
        _client?.Close();
        _client = null;
    }

    public void PlaceShipForHost(Coordinate start, int length, Orientation orientation)
    {
        _engine.PlaceShip(PlayerRole.Server, start, length, orientation);
        SnapshotUpdated?.Invoke(_engine.Snapshot(PlayerRole.Server));
        _ = SendStateToClientAsync(_cts?.Token ?? CancellationToken.None, "Размещение сервера обновлено");
    }

    public void CompletePlacementForHost()
    {
        _engine.CompletePlacement(PlayerRole.Server);
        SnapshotUpdated?.Invoke(_engine.Snapshot(PlayerRole.Server));
        _ = SendStateToClientAsync(_cts?.Token ?? CancellationToken.None, "Сервер готов");
    }

    public void ShootForHost(Coordinate coordinate)
    {
        if (_engine.Shoot(PlayerRole.Server, coordinate))
        {
            SnapshotUpdated?.Invoke(_engine.Snapshot(PlayerRole.Server));
            _ = SendStateToClientAsync(_cts?.Token ?? CancellationToken.None, "Ход сервера");
        }
    }

    private async Task SendStateToClientAsync(CancellationToken token, string status)
    {
        if (_writer == null)
        {
            return;
        }

        var snapshot = _engine.Snapshot(PlayerRole.Client, status);
        await SendAsync(Protocol.State(snapshot), token);
    }

    private async void OnStateChanged()
    {
        SnapshotUpdated?.Invoke(_engine.Snapshot(PlayerRole.Server));
        await SendStateToClientAsync(_cts?.Token ?? CancellationToken.None, _engine.LastStatus);
    }

    private async Task SendAsync(string line, CancellationToken token)
    {
        if (_writer == null)
        {
            return;
        }

        await _sendLock.WaitAsync(token);
        try
        {
            await _writer.WriteLineAsync(line);
        }
        catch (Exception ex)
        {
            Error?.Invoke($"Ошибка отправки: {ex.Message}");
        }
        finally
        {
            _sendLock.Release();
        }
    }

    private async void SendTimer(int seconds)
    {
        await SendAsync(Protocol.Timer(seconds), _cts?.Token ?? CancellationToken.None);
    }

    public async Task<string> SaveAsync(string? fileName = null)
    {
        return await GameStorage.SaveAsync(_engine.Session, fileName);
    }

    public async Task<bool> LoadAsync(string path)
    {
        var loaded = await GameStorage.LoadAsync(path);
        if (loaded == null)
        {
            return false;
        }

        _engine.LoadSession(loaded);
        SnapshotUpdated?.Invoke(_engine.Snapshot(PlayerRole.Server, "Сохранение загружено"));
        _ = SendStateToClientAsync(_cts?.Token ?? CancellationToken.None, "Состояние обновлено после загрузки");
        return true;
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _listener?.Stop();
        _reader?.Dispose();
        _writer?.Dispose();
        _client?.Dispose();
    }
}
