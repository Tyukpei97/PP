using TmSimulator.Core.Machine;

namespace TmSimulator.UI.ViewModels;

public class StateViewModel : ObservableObject
{
    private readonly State _state;

    public StateViewModel(State state)
    {
        _state = state;
    }

    public string Name
    {
        get => _state.Name;
        set
        {
            _state.Name = value;
            OnPropertyChanged();
        }
    }

    public bool IsStart
    {
        get => _state.IsStart;
        set
        {
            _state.IsStart = value;
            OnPropertyChanged();
        }
    }

    public bool IsHalting
    {
        get => _state.IsHalting;
        set
        {
            _state.IsHalting = value;
            OnPropertyChanged();
        }
    }

    public double X
    {
        get => _state.X;
        set
        {
            _state.X = value;
            OnPropertyChanged();
        }
    }

    public double Y
    {
        get => _state.Y;
        set
        {
            _state.Y = value;
            OnPropertyChanged();
        }
    }

    public State Model => _state;
}
