using TmSimulator.Core.Machine;

namespace TmSimulator.UI.ViewModels;

public class TransitionViewModel : ObservableObject
{
    private TransitionRule _rule;
    private readonly StateViewModel _fromState;
    private readonly StateViewModel _toState;

    public TransitionViewModel(TransitionRule rule, StateViewModel fromState, StateViewModel toState)
    {
        _rule = rule;
        _fromState = fromState;
        _toState = toState;
    }

    public string From => _rule.FromState;
    public string To => _rule.ToState;
    public char Read => _rule.ReadSymbol;
    public char Write => _rule.WriteSymbol;
    public Direction Move => _rule.Move;
    public TransitionRule Rule => _rule;
    public StateViewModel FromState => _fromState;
    public StateViewModel ToState => _toState;

    public string Label => $"{Read}->{Write},{MoveToString(Move)}";

    private static string MoveToString(Direction move) => move switch
    {
        Direction.Left => "L",
        Direction.Right => "R",
        _ => "S"
    };
}
