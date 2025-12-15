namespace TmSimulator.UI.ViewModels;

public class TapeCellViewModel : ObservableObject
{
    private long _position;
    private char _symbol;
    private bool _isHead;

    public long Position
    {
        get => _position;
        set => SetProperty(ref _position, value);
    }

    public char Symbol
    {
        get => _symbol;
        set => SetProperty(ref _symbol, value);
    }

    public bool IsHead
    {
        get => _isHead;
        set => SetProperty(ref _isHead, value);
    }
}
