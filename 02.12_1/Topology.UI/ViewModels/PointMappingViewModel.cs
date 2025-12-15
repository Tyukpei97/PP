namespace Topology.UI.ViewModels;

public class PointMappingViewModel : BaseViewModel
{
    public int SourceId { get; init; }
    public string SourceName { get; init; } = string.Empty;

    private int _targetId;
    public int TargetId
    {
        get => _targetId;
        set
        {
            if (_targetId == value) return;
            _targetId = value;
            OnPropertyChanged();
        }
    }
}
