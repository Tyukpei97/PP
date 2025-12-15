using System.Windows;

namespace NfaVisualDebugger.UI.ViewModels
{
    public class StateViewModel : ViewModelBase
    {
        private int _id;
        private string _name;
        private bool _isStart;
        private bool _isAccept;
        private double _x;
        private double _y;
        private bool _isActive;
        private bool _isSelected;

        public int Id
        {
            get => _id;
            private set => SetField(ref _id, value);
        }

        public string Name
        {
            get => _name;
            set => SetField(ref _name, value);
        }

        public bool IsStart
        {
            get => _isStart;
            set => SetField(ref _isStart, value);
        }

        public bool IsAccept
        {
            get => _isAccept;
            set => SetField(ref _isAccept, value);
        }

        public double X
        {
            get => _x;
            set => SetField(ref _x, value);
        }

        public double Y
        {
            get => _y;
            set => SetField(ref _y, value);
        }

        public bool IsActive
        {
            get => _isActive;
            set => SetField(ref _isActive, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetField(ref _isSelected, value);
        }

        public Point Position
        {
            get => new(X, Y);
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }

        public StateViewModel(int id, string name, bool isStart, bool isAccept, double x, double y)
        {
            _id = id;
            _name = name;
            _isStart = isStart;
            _isAccept = isAccept;
            _x = x;
            _y = y;
        }

        public void UpdateId(int id) => Id = id;
    }
}
