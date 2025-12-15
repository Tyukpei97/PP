using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TmSimulator.Core.Machine;

namespace TmSimulator.Core.Simulation;

public class TmRunner
{
    private readonly TmDefinition _definition;
    private readonly ConfigurationHasher _hasher = new();
    private readonly LoopDetector _loopDetector = new();

    public TapeModel Tape { get; private set; }
    public string CurrentState { get; private set; }
    public long HeadPosition { get; private set; }
    public int StepLimit { get; set; } = 10000;
    public int StepCount { get; private set; }
    public List<TraceEntry> Trace { get; } = new();

    public TmRunner(TmDefinition definition)
    {
        _definition = definition;
        Tape = new TapeModel(definition.Alphabet);
        CurrentState = definition.GetStartState()?.Name ?? string.Empty;
    }

    public bool Reset(string input, out string? error)
    {
        Tape = new TapeModel(_definition.Alphabet);
        StepCount = 0;
        HeadPosition = 0;
        Trace.Clear();
        _loopDetector.Reset();

        var start = _definition.GetStartState();
        if (start == null)
        {
            error = "Начальное состояние не задано.";
            return false;
        }

        CurrentState = start.Name;

        if (!Tape.TrySetInput(input, out error))
        {
            CurrentState = start.Name;
            return false;
        }

        var initialHash = _hasher.Hash(CurrentState, HeadPosition, Tape, _definition.Alphabet);
        _loopDetector.IsRepeated(initialHash); // добавляем первую конфигурацию
        return true;
    }

    public StepResult Step()
    {
        var stateBefore = CurrentState;
        var status = SimulationStatus.Running;
        string message = string.Empty;
        var read = Tape.Read(HeadPosition);
        var rule = _definition.FindRule(CurrentState, read);

        if (_definition.GetHaltingStates().Any(s => s.Name == CurrentState))
        {
            status = SimulationStatus.HaltedAccepting;
            message = "Достигнуто завершающее состояние.";
            var haltResult = new StepResult
            {
                StepIndex = StepCount,
                StateBefore = stateBefore,
                StateAfter = CurrentState,
                ReadSymbol = read,
                WriteSymbol = read,
                Move = Direction.Stay,
                HeadPosition = HeadPosition,
                Status = status,
                Message = message,
                Rule = null
            };
            Trace.Add(new TraceEntry
            {
                Step = haltResult.StepIndex,
                State = haltResult.StateAfter,
                Read = haltResult.ReadSymbol,
                Write = haltResult.WriteSymbol,
                Move = haltResult.Move,
                HeadPosition = haltResult.HeadPosition
            });
            return haltResult;
        }

        if (rule == null)
        {
            status = SimulationStatus.HaltedNoRule;
            message = "Нет правила для текущей пары (состояние, символ).";
            var missingRule = new StepResult
            {
                StepIndex = StepCount,
                StateBefore = stateBefore,
                StateAfter = CurrentState,
                ReadSymbol = read,
                WriteSymbol = read,
                Move = Direction.Stay,
                HeadPosition = HeadPosition,
                Status = status,
                Message = message,
                Rule = null
            };
            Trace.Add(new TraceEntry
            {
                Step = missingRule.StepIndex,
                State = missingRule.StateAfter,
                Read = missingRule.ReadSymbol,
                Write = missingRule.WriteSymbol,
                Move = missingRule.Move,
                HeadPosition = missingRule.HeadPosition
            });
            return missingRule;
        }

        Tape.Write(HeadPosition, rule.WriteSymbol);
        MoveHead(rule.Move);
        CurrentState = rule.ToState;
        StepCount++;

        var hash = _hasher.Hash(CurrentState, HeadPosition, Tape, _definition.Alphabet);
        if (_loopDetector.IsRepeated(hash))
        {
            status = SimulationStatus.LoopDetected;
            message = "Цикл обнаружен: конфигурация повторилась.";
        }
        else if (StepCount >= StepLimit)
        {
            status = SimulationStatus.StepLimitReached;
            message = "Достигнут лимит шагов.";
        }
        else if (_definition.GetHaltingStates().Any(s => s.Name == CurrentState))
        {
            status = SimulationStatus.HaltedAccepting;
            message = "Достигнуто завершающее состояние.";
        }

        var result = new StepResult
        {
            StepIndex = StepCount,
            StateBefore = stateBefore,
            StateAfter = CurrentState,
            ReadSymbol = read,
            WriteSymbol = rule.WriteSymbol,
            Move = rule.Move,
            HeadPosition = HeadPosition,
            Status = status,
            Message = message,
            Rule = rule
        };

        Trace.Add(new TraceEntry
        {
            Step = result.StepIndex,
            State = result.StateAfter,
            Read = result.ReadSymbol,
            Write = result.WriteSymbol,
            Move = result.Move,
            HeadPosition = result.HeadPosition
        });

        return result;
    }

    private void MoveHead(Direction move)
    {
        if (move == Direction.Left)
            HeadPosition--;
        else if (move == Direction.Right)
            HeadPosition++;
    }

    public async IAsyncEnumerable<StepResult> RunAsync([EnumeratorCancellation] CancellationToken token, int delayMilliseconds, bool stopOnHalt = true)
    {
        while (true)
        {
            token.ThrowIfCancellationRequested();
            var result = Step();
            yield return result;

            if (result.Status != SimulationStatus.Running)
            {
                if (stopOnHalt || result.Status is SimulationStatus.LoopDetected or SimulationStatus.StepLimitReached or SimulationStatus.HaltedNoRule)
                    yield break;
            }

            if (delayMilliseconds > 0)
                await Task.Delay(delayMilliseconds, token);
        }
    }
}
