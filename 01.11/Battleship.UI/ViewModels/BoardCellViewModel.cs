using System.Windows.Input;
using Battleship.Core.Models;
using Battleship.UI.ViewModels.Base;

namespace Battleship.UI.ViewModels;

public class BoardCellViewModel : ViewModelBase
{
    private CellState _state;
    private bool _isHighlighted;

    public int X { get; }
    public int Y { get; }
    public ICommand ClickCommand { get; }

    public CellState State
    {
        get => _state;
        set => SetField(ref _state, value);
    }

    public bool IsHighlighted
    {
        get => _isHighlighted;
        set => SetField(ref _isHighlighted, value);
    }

    public BoardCellViewModel(int x, int y, ICommand clickCommand)
    {
        X = x;
        Y = y;
        ClickCommand = clickCommand;
        _state = CellState.Unknown;
    }
}
