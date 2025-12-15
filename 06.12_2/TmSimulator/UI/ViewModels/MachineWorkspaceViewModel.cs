using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using TmSimulator.Core.Analysis;
using TmSimulator.Core.Machine;
using TmSimulator.Core.Parsing;
using TmSimulator.Core.Serialization;
using TmSimulator.Core.Simulation;

namespace TmSimulator.UI.ViewModels;

public class MachineWorkspaceViewModel : ObservableObject
{
    private readonly RuleParser _parser = new();
    private readonly JsonProjectSerializer _serializer = new();
    private readonly TestRunner _testRunner = new();
    private readonly TerminationHeuristics _heuristics = new();
    private readonly TraceExporter _traceExporter = new();

    private TmDefinition _definition;
    private TmRunner _runner;
    private CancellationTokenSource? _cts;
    private string _statusMessage = "Готово";
    private string _tapeInput = string.Empty;
    private string _rulesText = string.Empty;
    private int _stepLimit = 10000;
    private int _delayMs = 200;
    private bool _isRunning;
    private bool _deterministic = true;
    private readonly string _workspaceName;
    private string _newRuleFrom = string.Empty;
    private string _newRuleTo = string.Empty;
    private string _newRuleRead = "_";
    private string _newRuleWrite = "_";
    private Direction _newRuleMove = Direction.Stay;
    private bool _initialized;

    public ObservableCollection<StateViewModel> States { get; } = new();
    public ObservableCollection<TransitionViewModel> Rules { get; } = new();
    public ObservableCollection<char> AlphabetSymbols { get; } = new();
    public ObservableCollection<TapeCellViewModel> TapeCells { get; } = new();
    public ObservableCollection<TraceEntry> TraceEntries { get; } = new();
    public ObservableCollection<TestResultViewModel> TestResults { get; } = new();
    public ObservableCollection<string> HeuristicHints { get; } = new();

    public RelayCommand AddStateCommand { get; }
    public RelayCommand RemoveStateCommand { get; }
    public RelayCommand SetStartStateCommand { get; }
    public RelayCommand ToggleHaltingCommand { get; }
    public RelayCommand AddSymbolCommand { get; }
    public RelayCommand RemoveSymbolCommand { get; }
    public RelayCommand AddRuleCommand { get; }
    public RelayCommand ImportRulesCommand { get; }
    public RelayCommand StartCommand { get; }
    public RelayCommand PauseCommand { get; }
    public RelayCommand StepCommand { get; }
    public RelayCommand ResetCommand { get; }
    public RelayCommand FastForwardCommand { get; }
    public RelayCommand SaveProjectCommand { get; }
    public RelayCommand LoadProjectCommand { get; }
    public RelayCommand ExportTraceCommand { get; }
    public RelayCommand RunTestsCommand { get; }
    public RelayCommand ApplyNewRuleCommand { get; }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public string TapeInput
    {
        get => _tapeInput;
        set => SetProperty(ref _tapeInput, value);
    }

    public string RulesText
    {
        get => _rulesText;
        set => SetProperty(ref _rulesText, value);
    }

    public int StepLimit
    {
        get => _stepLimit;
        set => SetProperty(ref _stepLimit, value);
    }

    public int DelayMs
    {
        get => _delayMs;
        set => SetProperty(ref _delayMs, value);
    }

    public bool IsRunning
    {
        get => _isRunning;
        set => SetProperty(ref _isRunning, value);
    }

    public bool Deterministic
    {
        get => _deterministic;
        set => SetProperty(ref _deterministic, value);
    }

    public string NewRuleFrom
    {
        get => _newRuleFrom;
        set => SetProperty(ref _newRuleFrom, value);
    }

    public string NewRuleTo
    {
        get => _newRuleTo;
        set => SetProperty(ref _newRuleTo, value);
    }

    public string NewRuleRead
    {
        get => _newRuleRead;
        set => SetProperty(ref _newRuleRead, value);
    }

    public string NewRuleWrite
    {
        get => _newRuleWrite;
        set => SetProperty(ref _newRuleWrite, value);
    }

    public Direction NewRuleMove
    {
        get => _newRuleMove;
        set => SetProperty(ref _newRuleMove, value);
    }

    public MachineWorkspaceViewModel(string workspaceName)
    {
        _workspaceName = workspaceName;
        _definition = new TmDefinition(new Alphabet(new[] { '0', '1', '_' }));
        _definition.AddState("q0", out _, isStart: true, isHalting: false, x: 80, y: 80);
        _definition.AddState("HALT", out _, isStart: false, isHalting: true, x: 240, y: 120);
        _runner = new TmRunner(_definition);

        RefreshAlphabet();
        RefreshStates();
        RefreshRules();
        UpdateTapeCells();
        UpdateStatus("Готово");
        RefreshHeuristics();

        AddStateCommand = new RelayCommand(p => AddState(p as string ?? $"q{States.Count}"));
        RemoveStateCommand = new RelayCommand(p => RemoveState(p as string ?? string.Empty));
        SetStartStateCommand = new RelayCommand(p => SetStartState(p as string ?? string.Empty));
        ToggleHaltingCommand = new RelayCommand(p => ToggleHalting(p as string ?? string.Empty));
        AddSymbolCommand = new RelayCommand(p => AddSymbol(p as string ?? string.Empty));
        RemoveSymbolCommand = new RelayCommand(p => RemoveSymbol(p as string ?? string.Empty));
        AddRuleCommand = new RelayCommand(p => AddRuleFromObject(p));
        ApplyNewRuleCommand = new RelayCommand(_ => AddRuleFromFields());
        ImportRulesCommand = new RelayCommand(async _ => await ImportRulesAsync());
        StartCommand = new RelayCommand(async _ => await RunAsync());
        PauseCommand = new RelayCommand(_ => Pause());
        StepCommand = new RelayCommand(_ => StepOnce());
        ResetCommand = new RelayCommand(_ => ResetRunner());
        FastForwardCommand = new RelayCommand(async _ => await RunAsync(true));
        SaveProjectCommand = new RelayCommand(p => SaveProject(p as IEnumerable<string> ?? new List<string>()));
        LoadProjectCommand = new RelayCommand(p => LoadProject(p as ObservableCollection<string>));
        ExportTraceCommand = new RelayCommand(_ => ExportTrace());
        RunTestsCommand = new RelayCommand(p => RunTests(p as ObservableCollection<string> ?? new ObservableCollection<string>()));
    }

    private void UpdateStatus(string message)
    {
        StatusMessage = $"[{_workspaceName}] {message}";
    }

    private void RefreshAlphabet()
    {
        AlphabetSymbols.Clear();
        foreach (var s in _definition.Alphabet.Symbols.OrderBy(c => c))
        {
            AlphabetSymbols.Add(s);
        }
    }

    private void RefreshStates()
    {
        States.Clear();
        foreach (var s in _definition.States)
        {
            States.Add(new StateViewModel(s));
        }
        AutoLayoutStates();
        if (States.Any())
        {
            NewRuleFrom ??= States.First().Name;
            NewRuleTo ??= States.First().Name;
        }
    }

    private void RefreshRules()
    {
        Rules.Clear();
        foreach (var r in _definition.Rules)
        {
            var from = States.FirstOrDefault(s => s.Name == r.FromState);
            var to = States.FirstOrDefault(s => s.Name == r.ToState);
            if (from != null && to != null)
            {
                Rules.Add(new TransitionViewModel(r, from, to));
            }
        }
    }

    private void RefreshHeuristics()
    {
        HeuristicHints.Clear();
        foreach (var hint in _heuristics.GetHints(_definition))
        {
            HeuristicHints.Add(hint);
        }
    }

    private void AutoLayoutStates()
    {
        if (States.Count == 0) return;
        var centerX = 200;
        var centerY = 160;
        var radius = 120;
        for (int i = 0; i < States.Count; i++)
        {
            var angle = 2 * Math.PI * i / States.Count;
            States[i].X = centerX + radius * Math.Cos(angle);
            States[i].Y = centerY + radius * Math.Sin(angle);
        }
    }

    public void AddState(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return;
        if (!_definition.AddState(name.Trim(), out var error, false, false))
        {
            UpdateStatus(error ?? "Не удалось добавить состояние.");
            return;
        }

        RefreshStates();
        UpdateStatus($"Состояние {name} добавлено.");
        _initialized = false;
        RefreshHeuristics();
    }

    public void RemoveState(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return;
        _definition.RemoveState(name);
        RefreshStates();
        RefreshRules();
        UpdateStatus($"Состояние {name} удалено.");
        _initialized = false;
        RefreshHeuristics();
    }

    public void SetStartState(string name)
    {
        if (!_definition.TrySetStartState(name, out var error))
        {
            UpdateStatus(error ?? "Не удалось задать начальное состояние.");
            return;
        }

        foreach (var s in States)
        {
            s.IsStart = s.Name == name;
        }

        UpdateStatus($"Начальное состояние: {name}");
        _initialized = false;
        RefreshHeuristics();
    }

    public void ToggleHalting(string name)
    {
        if (!_definition.TryToggleHalting(name, out var error))
        {
            UpdateStatus(error ?? "Не удалось переключить завершающее состояние.");
            return;
        }

        RefreshStates();
        UpdateStatus($"Состояние {name} изменено.");
        _initialized = false;
        RefreshHeuristics();
    }

    public void AddSymbol(string symbolText)
    {
        if (string.IsNullOrWhiteSpace(symbolText)) return;
        var ch = symbolText.Trim()[0];
        _definition.Alphabet.Symbols.Add(ch);
        RefreshAlphabet();
        UpdateStatus($"Символ '{ch}' добавлен в алфавит.");
        _initialized = false;
        RefreshHeuristics();
    }

    public void RemoveSymbol(string symbolText)
    {
        if (string.IsNullOrWhiteSpace(symbolText)) return;
        var ch = symbolText.Trim()[0];
        if (ch == _definition.Alphabet.BlankSymbol)
        {
            UpdateStatus("Нельзя удалить символ пустой ленты.");
            return;
        }

        _definition.Alphabet.Symbols.Remove(ch);
        RefreshAlphabet();
        UpdateStatus($"Символ '{ch}' удалён.");
        _initialized = false;
        RefreshHeuristics();
    }

    private void AddRuleFromObject(object? param)
    {
        if (param is not TransitionRule rule)
            return;

        if (!_definition.TryAddOrUpdateRule(rule, Deterministic, out var error))
        {
            UpdateStatus(error ?? "Не удалось добавить правило.");
            return;
        }

        RefreshRules();
        UpdateStatus("Правило добавлено.");
        _initialized = false;
        RefreshHeuristics();
    }

    private void AddRuleFromFields()
    {
        if (string.IsNullOrWhiteSpace(NewRuleFrom) || string.IsNullOrWhiteSpace(NewRuleTo))
        {
            UpdateStatus("Укажите состояния для правила.");
            return;
        }

        var read = string.IsNullOrWhiteSpace(NewRuleRead) ? _definition.Alphabet.BlankSymbol : NewRuleRead.Trim()[0];
        var write = string.IsNullOrWhiteSpace(NewRuleWrite) ? _definition.Alphabet.BlankSymbol : NewRuleWrite.Trim()[0];
        var rule = new TransitionRule(NewRuleFrom.Trim(), read, NewRuleTo.Trim(), write, NewRuleMove);
        if (!_definition.TryAddOrUpdateRule(rule, Deterministic, out var error))
        {
            UpdateStatus(error ?? "Не удалось добавить правило.");
            return;
        }

        RefreshRules();
        UpdateStatus("Правило добавлено.");
        _initialized = false;
        RefreshHeuristics();
    }

    public async Task ImportRulesAsync()
    {
        var parsed = _parser.ParseWithValidation(RulesText, _definition, false, out var errors);
        if (errors.Any())
        {
            UpdateStatus(string.Join("; ", errors));
            return;
        }

        foreach (var rule in parsed)
        {
            _definition.TryAddOrUpdateRule(rule, Deterministic, out _);
        }

        RefreshRules();
        UpdateStatus("Правила импортированы.");
        _initialized = false;
        RefreshHeuristics();
        await Task.CompletedTask;
    }

    public bool ResetRunner()
    {
        var validation = _definition.Validate();
        if (validation.Any())
        {
            UpdateStatus(string.Join("; ", validation));
            return false;
        }

        _runner = new TmRunner(_definition)
        {
            StepLimit = StepLimit
        };

        if (!_runner.Reset(TapeInput ?? string.Empty, out var error))
        {
            UpdateStatus(error ?? "Не удалось подготовить ленту.");
            return false;
        }

        TraceEntries.Clear();
        TestResults.Clear();
        UpdateTapeCells();
        UpdateStatus("Лента подготовлена. Можно запускать.");
        _initialized = true;
        return true;
    }

    public void StepOnce()
    {
        if (!_initialized && !ResetRunner())
        {
            return;
        }

        var result = _runner.Step();
        TraceEntries.Add(new TraceEntry
        {
            Step = result.StepIndex,
            State = result.StateAfter,
            Read = result.ReadSymbol,
            Write = result.WriteSymbol,
            Move = result.Move,
            HeadPosition = result.HeadPosition
        });
        UpdateTapeCells();
        UpdateStatus(result.Message);
    }

    public async Task RunAsync(bool fast = false)
    {
        if (!ResetRunner())
            return;

        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        IsRunning = true;

        try
        {
            await foreach (var step in _runner.RunAsync(_cts.Token, fast ? 0 : DelayMs))
            {
                TraceEntries.Add(new TraceEntry
                {
                    Step = step.StepIndex,
                    State = step.StateAfter,
                    Read = step.ReadSymbol,
                    Write = step.WriteSymbol,
                    Move = step.Move,
                    HeadPosition = step.HeadPosition
                });
                UpdateTapeCells();
                UpdateStatus(step.Message);

                if (step.Status != SimulationStatus.Running)
                    break;
            }
        }
        catch (TaskCanceledException)
        {
            UpdateStatus("Выполнение остановлено.");
        }
        finally
        {
            IsRunning = false;
        }
    }

    public void Pause()
    {
        _cts?.Cancel();
        IsRunning = false;
        UpdateStatus("Пауза.");
    }

    private void UpdateTapeCells()
    {
        TapeCells.Clear();
        if (_runner == null) return;
        var window = _runner.Tape.Window(_runner.HeadPosition, 8);
        foreach (var cell in window)
        {
            TapeCells.Add(new TapeCellViewModel
            {
                Position = cell.position,
                Symbol = cell.symbol,
                IsHead = cell.position == _runner.HeadPosition
            });
        }
    }

    private void SaveProject(IEnumerable<string> tests)
    {
        var dialog = new SaveFileDialog
        {
            Filter = "Файл проекта (*.json)|*.json",
            FileName = $"{_workspaceName}.json"
        };

        if (dialog.ShowDialog() == true)
        {
            _serializer.Save(dialog.FileName, _definition, tests, TapeInput);
            UpdateStatus("Проект сохранён.");
        }
    }

    private void LoadProject(ObservableCollection<string>? tests)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Файл проекта (*.json)|*.json"
        };
        if (dialog.ShowDialog() == true)
        {
            if (_serializer.TryLoad(dialog.FileName, out var def, out var testsFromFile, out var lastInput, out var error))
            {
                if (def != null)
                {
                    _definition = def;
                    _runner = new TmRunner(_definition);
                    TapeInput = lastInput ?? string.Empty;
                    tests?.Clear();
                    foreach (var t in testsFromFile)
                    {
                        tests?.Add(t);
                    }
                    RefreshAlphabet();
                    RefreshStates();
                    RefreshRules();
                    UpdateStatus("Проект загружен.");
                    _initialized = false;
                    RefreshHeuristics();
                }
            }
            else
            {
                UpdateStatus(error ?? "Ошибка загрузки проекта.");
            }
        }
    }

    private void ExportTrace()
    {
        var dialog = new SaveFileDialog
        {
            Filter = "TXT|*.txt",
            FileName = $"{_workspaceName}_trace.txt"
        };
        if (dialog.ShowDialog() == true)
        {
            _traceExporter.SaveAsText(dialog.FileName, TraceEntries);
            UpdateStatus("Трассировка экспортирована.");
        }
    }

    private void RunTests(ObservableCollection<string> inputs)
    {
        TestResults.Clear();
        var results = _testRunner.RunMany(_definition, inputs, StepLimit);
        foreach (var r in results)
        {
            TestResults.Add(TestResultViewModel.FromResult(r));
        }

        UpdateStatus("Тесты выполнены.");
    }

    public IReadOnlyCollection<string> GetHeuristicHints() => _heuristics.GetHints(_definition);

    public TmDefinition GetDefinition() => _definition;
}
