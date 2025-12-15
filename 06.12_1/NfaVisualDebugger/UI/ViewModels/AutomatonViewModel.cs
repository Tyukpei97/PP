using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;
using NfaVisualDebugger.Core.Algorithms;
using NfaVisualDebugger.Core.Automata;
using NfaVisualDebugger.Core.Regex;

namespace NfaVisualDebugger.UI.ViewModels
{
    public class AutomatonViewModel : ViewModelBase
    {
        private readonly DispatcherTimer _timer;
        private readonly List<SimulationStep> _simulationSteps = new();

        private string _status = "Готово";
        private string _regexText = string.Empty;
        private string _inputText = string.Empty;
        private int _currentStepIndex = -1;
        private bool _simulationAccepted;
        private string _simulationMessage = "Симуляция не запускалась";
        private StateViewModel? _selectedState;
        private TransitionViewModel? _selectedTransition;
        private StateViewModel? _pendingFrom;

        public string Title { get; }

        public ObservableCollection<StateViewModel> States { get; } = new();
        public ObservableCollection<TransitionViewModel> Transitions { get; } = new();

        public ICommand AutoLayoutCommand { get; }
        public ICommand BuildFromRegexCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand StartSimulationCommand { get; }
        public ICommand StepForwardCommand { get; }
        public ICommand StepBackCommand { get; }
        public ICommand PauseCommand { get; }
        public ICommand ResetSimulationCommand { get; }

        public string Status
        {
            get => _status;
            set => SetField(ref _status, value);
        }

        public string RegexText
        {
            get => _regexText;
            set => SetField(ref _regexText, value);
        }

        public string InputText
        {
            get => _inputText;
            set => SetField(ref _inputText, value);
        }

        public int CurrentStepIndex
        {
            get => _currentStepIndex;
            set
            {
                if (_currentStepIndex != value)
                {
                    _currentStepIndex = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(StepInfo));
                    OnPropertyChanged(nameof(CurrentSymbol));
                }
            }
        }

        public bool SimulationAccepted
        {
            get => _simulationAccepted;
            set => SetField(ref _simulationAccepted, value);
        }

        public string SimulationMessage
        {
            get => _simulationMessage;
            set => SetField(ref _simulationMessage, value);
        }

        public string StepInfo =>
            _simulationSteps.Count > 0 && CurrentStepIndex >= 0
                ? $"Шаг {CurrentStepIndex} из {_simulationSteps.Count - 1}"
                : "Шаги отсутствуют";

        public string CurrentSymbol =>
            _simulationSteps.Count > 0 && CurrentStepIndex >= 0
                ? _simulationSteps[CurrentStepIndex].ConsumedSymbol ?? "ε"
                : string.Empty;

        public StateViewModel? SelectedState
        {
            get => _selectedState;
            set
            {
                if (_selectedState != null)
                {
                    _selectedState.IsSelected = false;
                }
                _selectedState = value;
                if (_selectedState != null)
                {
                    _selectedState.IsSelected = true;
                }
                OnPropertyChanged();
            }
        }

        public TransitionViewModel? SelectedTransition
        {
            get => _selectedTransition;
            set
            {
                if (_selectedTransition != null)
                {
                    _selectedTransition.IsSelected = false;
                }
                _selectedTransition = value;
                if (_selectedTransition != null)
                {
                    _selectedTransition.IsSelected = true;
                }
                OnPropertyChanged();
            }
        }

        public StateViewModel? PendingFromState
        {
            get => _pendingFrom;
            set => SetField(ref _pendingFrom, value);
        }

        public AutomatonViewModel(string title)
        {
            Title = title;
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(0.8)
            };
            _timer.Tick += (_, _) => StepForward();

            AutoLayoutCommand = new RelayCommand(_ => AutoLayout());
            BuildFromRegexCommand = new RelayCommand(_ => BuildFromRegex());
            ClearCommand = new RelayCommand(_ => Clear());
            StartSimulationCommand = new RelayCommand(_ => StartSimulation());
            StepForwardCommand = new RelayCommand(_ => StepForward(), _ => CanStepForward());
            StepBackCommand = new RelayCommand(_ => StepBack(), _ => CanStepBack());
            PauseCommand = new RelayCommand(_ => Pause());
            ResetSimulationCommand = new RelayCommand(_ => ResetSimulation());
        }

        public StateViewModel AddState(double x, double y)
        {
            NormalizeStateIds();
            var id = States.Count;
            var state = new StateViewModel(id, $"С{id}", States.Count == 0, false, x, y);
            States.Add(state);
            Status = $"Добавлено состояние {state.Name}";
            return state;
        }

        public void RemoveState(StateViewModel state)
        {
            var transitionsToRemove = Transitions.Where(t => t.From == state || t.To == state).ToList();
            foreach (var t in transitionsToRemove)
            {
                Transitions.Remove(t);
            }
            States.Remove(state);
            NormalizeStateIds();
            UpdateParallelIndices();
        }

        public void AddTransition(StateViewModel from, StateViewModel to, string label)
        {
            label = (label ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(label))
            {
                Status = "Метка перехода не может быть пустой (используйте Оч для эпсилон)";
                return;
            }
            if (label == "ε" || label.Equals("eps", StringComparison.OrdinalIgnoreCase))
            {
                label = Nfa.Epsilon;
            }

            var id = Transitions.Count;
            var transition = new TransitionViewModel(id, from, to, label);
            Transitions.Add(transition);
            UpdateParallelIndices();
            Status = $"Создан переход {from.Name} -> {to.Name} [{label}]";
        }

        public void UpdateTransitionLabel(TransitionViewModel transition, string label)
        {
            if (string.IsNullOrWhiteSpace(label))
            {
                Status = "Пустая метка не допускается";
                return;
            }

            transition.Label = label.Trim();
            Status = "Метка перехода обновлена";
            UpdateParallelIndices();
        }

        public void NormalizeStateIds()
        {
            for (int i = 0; i < States.Count; i++)
            {
                States[i].UpdateId(i);
            }
        }

        public void Clear()
        {
            States.Clear();
            Transitions.Clear();
            SelectedState = null;
            SelectedTransition = null;
            ResetSimulation();
            Status = "Поле очищено";
        }

        public void BuildFromRegex()
        {
            try
            {
                var ast = new RegexParser(RegexText).Parse();
                var builder = new ThompsonBuilder();
                var nfa = builder.Build(ast);
                LoadFromModel(nfa);
                AutoLayout();
                Status = "НКА построен по регулярному выражению";
            }
            catch (RegexParseException ex)
            {
                Status = $"Ошибка разбора: {ex.Message} (позиция {ex.Position})";
            }
        }

        public Nfa ToModel()
        {
            NormalizeStateIds();
            var nfa = new Nfa();
            foreach (var s in States.OrderBy(s => s.Id))
            {
                nfa.States.Add(new NfaState(s.Id, s.Name, s.IsStart, s.IsAccept, s.X, s.Y));
            }

            foreach (var t in Transitions)
            {
                nfa.Transitions.Add(new NfaTransition(t.From.Id, t.To.Id, t.Label));
            }

            return nfa;
        }

        public void LoadFromModel(Nfa nfa)
        {
            States.Clear();
            Transitions.Clear();
            foreach (var s in nfa.States.OrderBy(s => s.Id))
            {
                States.Add(new StateViewModel(s.Id, s.Name, s.IsStart, s.IsAccept, s.X, s.Y));
            }

            foreach (var t in nfa.Transitions)
            {
                var from = States.First(s => s.Id == t.FromStateId);
                var to = States.First(s => s.Id == t.ToStateId);
                Transitions.Add(new TransitionViewModel(Transitions.Count, from, to, t.Label));
            }

            UpdateParallelIndices();
        }

        public void AutoLayout(double width = 900, double height = 500)
        {
            if (States.Count == 0)
            {
                return;
            }

            var radius = Math.Min(width, height) / 2 - 60;
            var centerX = width / 2;
            var centerY = height / 2;
            var angleStep = 2 * Math.PI / States.Count;

            for (int i = 0; i < States.Count; i++)
            {
                var angle = i * angleStep;
                States[i].X = centerX + radius * Math.Cos(angle);
                States[i].Y = centerY + radius * Math.Sin(angle);
            }
        }

        public void StartTransition(StateViewModel from)
        {
            PendingFromState = from;
            Status = $"Начало создания перехода из {from.Name}";
        }

        public void FinishTransition(StateViewModel to, string label)
        {
            if (PendingFromState == null)
                return;
            AddTransition(PendingFromState, to, label);
            PendingFromState = null;
        }

        public void StartSimulation()
        {
            var model = ToModel();
            var errors = AutomatonValidator.Validate(model);
            var blocking = errors.Where(e => !e.StartsWith("Предупреждение")).ToList();
            if (blocking.Count > 0)
            {
                Status = blocking.First();
                return;
            }

            if (errors.Count > 0)
            {
                Status = errors.First();
            }

            var result = NfaSimulator.Run(model, InputText ?? string.Empty);
            _simulationSteps.Clear();
            _simulationSteps.AddRange(result.Steps);
            SimulationAccepted = result.Accepted;
            SimulationMessage = result.Accepted ? "Строка принимается" : "Строка отвергается";
            CurrentStepIndex = 0;
            ApplyStep();
            _timer.Start();
            Status = "Симуляция запущена";
            UpdateSimulationCommands();
        }

        public void Pause()
        {
            _timer.Stop();
            Status = "Пауза";
        }

        public void ResetSimulation()
        {
            _timer.Stop();
            _simulationSteps.Clear();
            foreach (var s in States)
            {
                s.IsActive = false;
            }
            foreach (var t in Transitions)
            {
                t.IsHighlighted = false;
            }
            CurrentStepIndex = -1;
            SimulationMessage = "Симуляция не запускалась";
            Status = "Сброс симуляции";
            UpdateSimulationCommands();
        }

        public void StepForward()
        {
            if (!CanStepForward())
            {
                _timer.Stop();
                return;
            }

            CurrentStepIndex++;
            ApplyStep();

            if (CurrentStepIndex >= _simulationSteps.Count - 1)
            {
                _timer.Stop();
                Status = SimulationMessage;
            }
            UpdateSimulationCommands();
        }

        public void StepBack()
        {
            if (!CanStepBack())
            {
                return;
            }

            CurrentStepIndex--;
            ApplyStep();
            UpdateSimulationCommands();
        }

        private bool CanStepForward() => _simulationSteps.Count > 0 && CurrentStepIndex < _simulationSteps.Count - 1;
        private bool CanStepBack() => _simulationSteps.Count > 0 && CurrentStepIndex > 0;

        private void ApplyStep()
        {
            if (CurrentStepIndex < 0 || CurrentStepIndex >= _simulationSteps.Count)
            {
                return;
            }

            foreach (var s in States)
            {
                s.IsActive = false;
            }
            foreach (var t in Transitions)
            {
                t.IsHighlighted = false;
            }

            var step = _simulationSteps[CurrentStepIndex];
            foreach (var stateId in step.ActiveStates)
            {
                var vm = States.FirstOrDefault(s => s.Id == stateId);
                if (vm != null)
                {
                    vm.IsActive = true;
                }
            }

            foreach (var t in step.HighlightedTransitions)
            {
                var vm = Transitions.FirstOrDefault(x => x.From.Id == t.FromStateId && x.To.Id == t.ToStateId && x.Label == t.Label);
                if (vm != null)
                {
                    vm.IsHighlighted = true;
                }
            }

            OnPropertyChanged(nameof(StepInfo));
            OnPropertyChanged(nameof(CurrentSymbol));
        }

        private void UpdateParallelIndices()
        {
            var groups = Transitions
                .GroupBy(t => (t.From.Id, t.To.Id))
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var g in groups.Values)
            {
                for (int i = 0; i < g.Count; i++)
                {
                    g[i].ParallelIndex = i;
                }
            }
        }

        private void UpdateSimulationCommands()
        {
            (StepForwardCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (StepBackCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }
}
