using NfaVisualDebugger.Core.Algorithms;

namespace NfaVisualDebugger.UI.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private const int DfaLimit = 2000;

        private string _equivalenceMessage = "Сравнение не выполнялось";
        private string _counterExample = string.Empty;

        public AutomatonViewModel AutomatonA { get; }
        public AutomatonViewModel AutomatonB { get; }

        public string EquivalenceMessage
        {
            get => _equivalenceMessage;
            set => SetField(ref _equivalenceMessage, value);
        }

        public string CounterExample
        {
            get => _counterExample;
            set => SetField(ref _counterExample, value);
        }

        public RelayCommand CompareCommand { get; }

        public MainViewModel()
        {
            AutomatonA = new AutomatonViewModel("Автомат A");
            AutomatonB = new AutomatonViewModel("Автомат B");
            CompareCommand = new RelayCommand(_ => Compare());
        }

        public void Compare()
        {
            var result = EquivalenceChecker.Check(AutomatonA.ToModel(), AutomatonB.ToModel(), DfaLimit);
            EquivalenceMessage = result.Message;
            if (result.CounterExample != null)
            {
                CounterExample = $"Отличающий пример: '{result.CounterExample.Word}' (A принимает: {result.CounterExample.AcceptedByA}, B принимает: {result.CounterExample.AcceptedByB})";
            }
            else
            {
                CounterExample = string.Empty;
            }
        }
    }
}
