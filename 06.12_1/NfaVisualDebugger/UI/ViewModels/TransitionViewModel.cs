namespace NfaVisualDebugger.UI.ViewModels
{
    public class TransitionViewModel : ViewModelBase
    {
        private string _label;
        private bool _isHighlighted;
        private bool _isSelected;
        private int _parallelIndex;

        public int Id { get; }
        public StateViewModel From { get; }
        public StateViewModel To { get; }

        public string Label
        {
            get => _label;
            set => SetField(ref _label, value);
        }

        public bool IsHighlighted
        {
            get => _isHighlighted;
            set => SetField(ref _isHighlighted, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetField(ref _isSelected, value);
        }

        public int ParallelIndex
        {
            get => _parallelIndex;
            set => SetField(ref _parallelIndex, value);
        }

        public bool IsSelfLoop => From.Id == To.Id;

        public TransitionViewModel(int id, StateViewModel from, StateViewModel to, string label, int parallelIndex = 0)
        {
            Id = id;
            From = from;
            To = to;
            _label = label;
            _parallelIndex = parallelIndex;
        }
    }
}
