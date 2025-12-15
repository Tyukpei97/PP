using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Battleship.Core.Models;
using Battleship.UI.Services;
using Battleship.UI.ViewModels.Base;

namespace Battleship.UI.ViewModels;

public class BattleViewModel : ViewModelBase
{
    private readonly GameCoordinator _coordinator;
    private readonly Action _goStart;
    private string _status = "Ожидание хода";
    private string _turnText = string.Empty;
    private string _timerText = "Таймер: --";
    private string _statsText = string.Empty;
    private bool _isMyTurn;
    private bool _isBattleActive;
    private bool _isHost;
    private string _sessionId = string.Empty;
    private string _reconnectStatus = string.Empty;
    private string _selectedSave = string.Empty;
    private ObservableCollection<string> _saveFiles = new();

    public BoardViewModel OwnBoard { get; }
    public BoardViewModel OpponentBoard { get; }

    public string Status
    {
        get => _status;
        set => SetField(ref _status, value);
    }

    public string TurnText
    {
        get => _turnText;
        set => SetField(ref _turnText, value);
    }

    public string TimerText
    {
        get => _timerText;
        set => SetField(ref _timerText, value);
    }

    public string StatsText
    {
        get => _statsText;
        set => SetField(ref _statsText, value);
    }

    public bool CanShoot => _isBattleActive && _isMyTurn;
    public bool ShowSaves => _isHost;
    public string SessionId
    {
        get => _sessionId;
        set => SetField(ref _sessionId, value);
    }

    public string ReconnectStatus
    {
        get => _reconnectStatus;
        set => SetField(ref _reconnectStatus, value);
    }

    public ObservableCollection<string> SaveFiles
    {
        get => _saveFiles;
        set => SetField(ref _saveFiles, value);
    }

    public string SelectedSave
    {
        get => _selectedSave;
        set => SetField(ref _selectedSave, value);
    }

    public ICommand SaveCommand { get; }
    public ICommand LoadCommand { get; }
    public ICommand BackCommand { get; }
    public ICommand ReconnectCommand { get; }

    public BattleViewModel(GameCoordinator coordinator, Action goStart)
    {
        _coordinator = coordinator;
        _goStart = goStart;
        OwnBoard = new BoardViewModel(_ => { });
        OpponentBoard = new BoardViewModel(OnOpponentClick);

        SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => _isHost);
        LoadCommand = new RelayCommand(async _ => await LoadAsync(), _ => _isHost && !string.IsNullOrWhiteSpace(SelectedSave));
        BackCommand = new RelayCommand(_ => LeaveToStart());
        ReconnectCommand = new RelayCommand(async _ => await ReconnectAsync(), _ => !_isHost);
    }

    private void OnOpponentClick(Coordinate coordinate)
    {
        if (!CanShoot) return;
        _coordinator.Shoot(coordinate);
    }

    public void Update(GameStateSnapshot snapshot)
    {
        _isHost = _coordinator.IsHost;
        SessionId = snapshot.SessionId.ToString();
        _isBattleActive = snapshot.Phase == GamePhase.Battle;
        _isMyTurn = snapshot.Turn == snapshot.Viewer;
        Status = snapshot.Status;
        TurnText = _isMyTurn ? "Ваш ход" : "Ход соперника";
        TimerText = $"Таймер: {snapshot.TurnSecondsLeft} c";
        StatsText = $"Выстрелов: {snapshot.You.Statistics.Shots} | Попаданий: {snapshot.You.Statistics.Hits} | Потоплено: {snapshot.You.Statistics.Sunk} | Точность: {snapshot.You.Statistics.Accuracy:0}%";

        OwnBoard.Update(snapshot.OwnBoard);
        OpponentBoard.Update(snapshot.OpponentBoard);
        OnPropertyChanged(nameof(CanShoot));
        OnPropertyChanged(nameof(ShowSaves));
        (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (LoadCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (ReconnectCommand as RelayCommand)?.RaiseCanExecuteChanged();

        if (_isHost)
        {
            SaveFiles = new ObservableCollection<string>(_coordinator.GetSaveFiles());
            if (SaveFiles.Count > 0 && string.IsNullOrWhiteSpace(SelectedSave))
            {
                SelectedSave = SaveFiles.Last();
            }
        }
    }

    private async System.Threading.Tasks.Task SaveAsync()
    {
        await _coordinator.SaveAsync();
        SaveFiles = new ObservableCollection<string>(_coordinator.GetSaveFiles());
        if (SaveFiles.Count > 0)
        {
            SelectedSave = SaveFiles.Last();
        }
    }

    private async System.Threading.Tasks.Task LoadAsync()
    {
        if (string.IsNullOrWhiteSpace(SelectedSave)) return;
        await _coordinator.LoadAsync(SelectedSave);
    }

    private void LeaveToStart()
    {
        _coordinator.Stop();
        _goStart();
    }

    private async System.Threading.Tasks.Task ReconnectAsync()
    {
        if (_coordinator.LastHost == null || SessionId == string.Empty)
        {
            ReconnectStatus = "Нет данных для переподключения";
            return;
        }

        var ok = await _coordinator.ReconnectAsync(_coordinator.LastHost, _coordinator.LastPort, Guid.Parse(SessionId));
        ReconnectStatus = ok ? "Запрос отправлен" : "Переподключение не удалось";
    }
}
