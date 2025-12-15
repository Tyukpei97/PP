using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Battleship.Core.Models;
using Battleship.UI.ViewModels.Base;

namespace Battleship.UI.ViewModels;

public class BoardViewModel : ViewModelBase
{
    public ObservableCollection<BoardCellViewModel> Cells { get; }
    public ICommand CellCommand { get; }

    public BoardViewModel(Action<Coordinate> onCellClick)
    {
        CellCommand = new RelayCommand(param =>
        {
            if (param is Coordinate coord)
            {
                onCellClick(coord);
            }
            else if (param is BoardCellViewModel cell)
            {
                onCellClick(new Coordinate(cell.X, cell.Y));
            }
        });

        Cells = new ObservableCollection<BoardCellViewModel>(
            Enumerable.Range(0, Board.Size)
                .SelectMany(y => Enumerable.Range(0, Board.Size)
                    .Select(x => new BoardCellViewModel(x, y, CellCommand))));
    }

    public void Update(BoardSnapshot snapshot)
    {
        foreach (var cell in Cells)
        {
            var state = snapshot.Cells[cell.Y][cell.X];
            cell.State = state;
        }
    }
}
