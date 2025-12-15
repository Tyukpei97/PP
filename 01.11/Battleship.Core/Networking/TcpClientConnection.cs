using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Battleship.Core.Models;

namespace Battleship.Core.Networking;

public class TcpClientConnection : IDisposable
{
    private TcpClient? _client;
    private StreamReader? _reader;
    private StreamWriter? _writer;
    private CancellationTokenSource? _cts;
    private Task? _readLoop;

    public event Action<GameStateSnapshot>? SnapshotReceived;
    public event Action<string>? Status;
    public event Action<string>? Error;
    public event Action<int>? Timer;

    public GameStateSnapshot? LastSnapshot { get; private set; }

    public async Task<bool> ConnectAsync(string host, int port, string nickname)
    {
        _cts = new CancellationTokenSource();
        _client = new TcpClient();
        await _client.ConnectAsync(host, port);
        var stream = _client.GetStream();
        _reader = new StreamReader(stream, Encoding.UTF8);
        _writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

        await SendAsync(Protocol.Hello(nickname));
        _readLoop = Task.Run(ReadLoopAsync, _cts.Token);
        Status?.Invoke("Соединение установлено");
        return true;
    }

    public async Task<bool> ReconnectAsync(string host, int port, Guid sessionId)
    {
        _cts = new CancellationTokenSource();
        _client = new TcpClient();
        await _client.ConnectAsync(host, port);
        var stream = _client.GetStream();
        _reader = new StreamReader(stream, Encoding.UTF8);
        _writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
        await SendAsync(Protocol.Reconnect(sessionId));
        _readLoop = Task.Run(ReadLoopAsync, _cts.Token);
        Status?.Invoke("Запрос на переподключение отправлен");
        return true;
    }

    private async Task ReadLoopAsync()
    {
        try
        {
            while (_reader != null && !_cts!.IsCancellationRequested)
            {
                var line = await _reader.ReadLineAsync();
                if (line == null)
                {
                    Status?.Invoke("Соединение закрыто");
                    return;
                }

                if (!Protocol.TryParse(line, out var message))
                {
                    continue;
                }

                await HandleMessageAsync(message);
            }
        }
        catch (Exception ex)
        {
            Error?.Invoke($"Сетевой сбой: {ex.Message}");
        }
    }

    private Task HandleMessageAsync(ProtocolMessage message)
    {
        switch (message.Type.ToUpperInvariant())
        {
            case "STATE":
                if (message.Parts.Length >= 1 && Protocol.TryDecodeState(message.Parts[0], out var snapshot) && snapshot != null)
                {
                    LastSnapshot = snapshot;
                    SnapshotReceived?.Invoke(snapshot);
                }
                break;
            case "ERROR":
                if (message.Parts.Length >= 1)
                {
                    Error?.Invoke(message.Parts[0]);
                }
                break;
            case "TIMER":
                if (message.Parts.Length >= 1 && int.TryParse(message.Parts[0], out var seconds))
                {
                    Timer?.Invoke(seconds);
                }
                break;
            case "HELLO":
            case "ROLE":
            case "PHASE":
            case "TURN":
                Status?.Invoke(string.Join(' ', message.Parts));
                break;
        }

        return Task.CompletedTask;
    }

    public async Task SendPlacementAsync(IEnumerable<PlacedShip> ships)
    {
        var payload = PlacementCodec.Encode(ships);
        await SendAsync(Protocol.Build("PLACE", payload));
    }

    public async Task SendShotAsync(Coordinate coordinate)
    {
        await SendAsync(Protocol.Shot(coordinate.X, coordinate.Y));
    }

    private async Task SendAsync(string line)
    {
        if (_writer == null) return;
        try
        {
            await _writer.WriteLineAsync(line);
        }
        catch (Exception ex)
        {
            Error?.Invoke($"Не удалось отправить: {ex.Message}");
        }
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _reader?.Dispose();
        _writer?.Dispose();
        _client?.Dispose();
    }
}
