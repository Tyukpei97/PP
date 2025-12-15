using Battleship.UI.Services;
using Battleship.UI.ViewModels.Base;

namespace Battleship.UI.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly GameCoordinator _coordinator = new();
    private ViewModelBase _current;
    private string _globalStatus = "Готово";

    public StartViewModel Start { get; }
    public PlacementViewModel Placement { get; }
    public BattleViewModel Battle { get; }

    public ViewModelBase Current
    {
        get => _current;
        set => SetField(ref _current, value);
    }

    public string GlobalStatus
    {
        get => _globalStatus;
        set => SetField(ref _globalStatus, value);
    }

    public MainViewModel()
    {
        Battle = new BattleViewModel(_coordinator, GoStart);
        Placement = new PlacementViewModel(_coordinator, GoBattle);
        Start = new StartViewModel(_coordinator, GoPlacement);
        _current = Start;

        Start.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(Start.Status))
            {
                GlobalStatus = Start.Status;
            }
        };

        _coordinator.SnapshotChanged += snapshot =>
        {
            Placement.OnSnapshot(snapshot);
            Battle.Update(snapshot);
            GlobalStatus = snapshot.Status;
        };

        _coordinator.Status += msg => GlobalStatus = msg;
        _coordinator.Error += msg => GlobalStatus = msg;
    }

    private void GoPlacement()
    {
        Placement.ResetPlacement();
        Current = Placement;
    }
    private void GoBattle() => Current = Battle;
    private void GoStart() => Current = Start;
}
