using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Battleship.Core.Models;
using Battleship.UI.Services;
using Battleship.UI.ViewModels.Base;

namespace Battleship.UI.ViewModels;

public class PlacementViewModel : ViewModelBase
{
    private readonly GameCoordinator _coordinator;
    private readonly Action _goBattle;
    private readonly Board _localBoard = new();
    private readonly Dictionary<int, int> _limits = new() { { 4, 1 }, { 3, 2 }, { 2, 3 }, { 1, 4 } };
    private readonly Dictionary<int, int> _current = new() { { 4, 0 }, { 3, 0 }, { 2, 0 }, { 1, 0 } };
    private readonly List<PlacedShip> _ships = new();

    private int _selectedLength = 4;
    private bool _isHorizontal = true;
    private string _status = "Расставьте корабли";
    private bool _readySent;

    public BoardViewModel OwnBoard { get; }

    public int SelectedLength
    {
        get => _selectedLength;
        set => SetField(ref _selectedLength, value);
    }

    public bool IsHorizontal
    {
        get => _isHorizontal;
        set => SetField(ref _isHorizontal, value);
    }

    public string Status
    {
        get => _status;
        set => SetField(ref _status, value);
    }

    public bool CanFinish => _limits.All(kv => _current[kv.Key] == kv.Value);

    public string FleetInfo => $"4-палубный: {_current[4]}/1   3-палубный: {_current[3]}/2   2-палубный: {_current[2]}/3   1-палубный: {_current[1]}/4";

    public ICommand ResetCommand { get; }
    public ICommand ReadyCommand { get; }

    public PlacementViewModel(GameCoordinator coordinator, Action goBattle)
    {
        _coordinator = coordinator;
        _goBattle = goBattle;
        OwnBoard = new BoardViewModel(OnCellClick);
        ResetCommand = new RelayCommand(_ => ResetPlacement());
        ReadyCommand = new RelayCommand(_ => SendReady(), _ => CanFinish && !_readySent);
        UpdateBoardVisual();
    }

    private void OnCellClick(Coordinate coordinate)
    {
        if (_readySent) return;
        if (_current[_selectedLength] >= _limits[_selectedLength])
        {
            Status = "Лимит таких кораблей исчерпан";
            return;
        }

        var orientation = _isHorizontal ? Orientation.Horizontal : Orientation.Vertical;
        if (!_localBoard.CanPlaceShip(coordinate, _selectedLength, orientation, out var reason))
        {
            Status = reason ?? "Нельзя поставить сюда";
            return;
        }

        _localBoard.PlaceShip(coordinate, _selectedLength, orientation);
        _ships.Add(new PlacedShip(coordinate, _selectedLength, orientation));
        _current[_selectedLength]++;
        Status = $"Корабль длины {_selectedLength} установлен";
        UpdateBoardVisual();
        OnPropertyChanged(nameof(CanFinish));
        OnPropertyChanged(nameof(FleetInfo));
        (ReadyCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }

    private void SendReady()
    {
        if (!CanFinish)
        {
            Status = "Разместите все корабли";
            return;
        }

        _coordinator.ApplyPlacement(_ships);
        _readySent = true;
        Status = "Размещение отправлено на сервер";
        (ReadyCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }

    public void ResetPlacement()
    {
        _ships.Clear();
        _current[1] = _current[2] = _current[3] = _current[4] = 0;
        _localBoard.Reset();
        _readySent = false;
        Status = "Размещение сброшено";
        UpdateBoardVisual();
        OnPropertyChanged(nameof(CanFinish));
        OnPropertyChanged(nameof(FleetInfo));
        (ReadyCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }

    private void UpdateBoardVisual()
    {
        var snapshot = BoardSnapshot.ForOwner(_localBoard);
        OwnBoard.Update(snapshot);
    }

    public void OnSnapshot(GameStateSnapshot snapshot)
    {
        _readySent = snapshot.You.Ready;
        if (snapshot.Phase == GamePhase.Battle || snapshot.Phase == GamePhase.GameOver)
        {
            _goBattle();
        }

        Status = snapshot.Status;
        if (snapshot.Viewer == PlayerRole.Server || snapshot.Viewer == PlayerRole.Client)
        {
            OwnBoard.Update(snapshot.OwnBoard);
        }

        (ReadyCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }
}
