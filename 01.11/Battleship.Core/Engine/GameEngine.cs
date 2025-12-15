using System;
using System.Threading;
using Battleship.Core.Models;

namespace Battleship.Core.Engine;

public class GameEngine : IDisposable
{
    private readonly object _sync = new();
    private readonly Timer _timer;

    public GameSession Session { get; private set; }
    public string LastStatus { get; private set; } = "Ожидание";

    public event Action? StateChanged;
    public event Action<string>? Info;
    public event Action<string>? Error;
    public event Action<int>? TimerTick;

    public GameEngine(GameSession? session = null)
    {
        Session = session ?? new GameSession();
        _timer = new Timer(OnTimer, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
    }

    private void OnTimer(object? state)
    {
        lock (_sync)
        {
            var prevTurn = Session.Turn;
            Session.TickTimer();
            if (Session.Turn != prevTurn)
            {
                LastStatus = "Время вышло, ход передан сопернику";
            }

            TimerTick?.Invoke(Session.TurnSecondsLeft);
            StateChanged?.Invoke();
        }
    }

    public GameStateSnapshot Snapshot(PlayerRole viewer, string? customStatus = null)
    {
        lock (_sync)
        {
            return Session.CreateSnapshot(viewer, customStatus ?? LastStatus);
        }
    }

    public bool PlaceShip(PlayerRole role, Coordinate start, int length, Orientation orientation)
    {
        lock (_sync)
        {
            var (ok, error) = Session.TryPlaceShip(role, start, length, orientation);
            if (!ok)
            {
                Error?.Invoke(error ?? "Ошибка размещения");
                return false;
            }

            LastStatus = $"Корабль {length}-палубный установлен";
            StateChanged?.Invoke();
            return true;
        }
    }

    public bool CompletePlacement(PlayerRole role)
    {
        lock (_sync)
        {
            var (ok, error) = Session.TryCompletePlacement(role);
            if (!ok)
            {
                Error?.Invoke(error ?? "Не все корабли размещены");
                return false;
            }

            LastStatus = Session.Phase == GamePhase.Battle
                ? $"Бой начинается. Ход: {Session.Turn}"
                : "Ожидание соперника";

            StateChanged?.Invoke();
            return true;
        }
    }

    public bool Shoot(PlayerRole shooter, Coordinate coordinate)
    {
        lock (_sync)
        {
            var (result, error) = Session.TryProcessShot(shooter, coordinate);
            if (result == null)
            {
                Error?.Invoke(error ?? "Выстрел отклонён");
                return false;
            }

            switch (result.Outcome)
            {
                case ShotOutcome.Miss:
                    LastStatus = $"Мимо ({coordinate.X + 1},{coordinate.Y + 1})";
                    break;
                case ShotOutcome.Hit:
                    LastStatus = $"Попадание ({coordinate.X + 1},{coordinate.Y + 1})";
                    break;
                case ShotOutcome.Sunk:
                    LastStatus = $"Корабль потоплен ({coordinate.X + 1},{coordinate.Y + 1})";
                    break;
            }

            if (Session.Phase == GamePhase.GameOver)
            {
                var winnerName = Session.GetPlayer(shooter).Nickname;
                if (string.IsNullOrWhiteSpace(winnerName))
                {
                    winnerName = shooter == PlayerRole.Server ? "Сервер" : "Клиент";
                }

                LastStatus = $"Игра окончена. Победил {winnerName}";
            }

            StateChanged?.Invoke();
            return true;
        }
    }

    public void LoadSession(GameSession session)
    {
        lock (_sync)
        {
            Session = session;
            LastStatus = "Сессия загружена";
            StateChanged?.Invoke();
        }
    }

    public void ResetPlacement(PlayerRole role)
    {
        lock (_sync)
        {
            PlacementRules.ResetFleet(Session.GetPlayer(role));
            LastStatus = "Размещение сброшено";
            StateChanged?.Invoke();
        }
    }

    public void Dispose()
    {
        _timer.Dispose();
    }
}
