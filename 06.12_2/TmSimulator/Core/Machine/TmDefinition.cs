using System.Collections.Generic;
using System.Linq;

namespace TmSimulator.Core.Machine;

public class TmDefinition
{
    public Alphabet Alphabet { get; private set; }
    public List<State> States { get; } = new();
    private readonly Dictionary<(string, char), TransitionRule> _rules = new();

    public TmDefinition(Alphabet alphabet)
    {
        Alphabet = alphabet;
    }

    public IEnumerable<TransitionRule> Rules => _rules.Values;

    public void SetAlphabet(Alphabet alphabet)
    {
        Alphabet = alphabet;
    }

    public bool AddState(string name, out string? error, bool isStart = false, bool isHalting = false, double x = 0, double y = 0)
    {
        if (States.Any(s => s.Name == name))
        {
            error = "Состояние с таким именем уже существует.";
            return false;
        }

        if (isStart)
        {
            foreach (var s in States)
            {
                s.IsStart = false;
            }
        }

        States.Add(new State(name, isStart, isHalting, x, y));
        error = null;
        return true;
    }

    public void RemoveState(string name)
    {
        States.RemoveAll(s => s.Name == name);
        var keys = _rules.Keys.Where(k => k.Item1 == name || _rules[k].ToState == name).ToList();
        foreach (var k in keys)
        {
            _rules.Remove(k);
        }
    }

    public State? GetStartState() => States.FirstOrDefault(s => s.IsStart);

    public IEnumerable<State> GetHaltingStates() => States.Where(s => s.IsHalting);

    public bool TrySetStartState(string name, out string? error)
    {
        var state = States.FirstOrDefault(s => s.Name == name);
        if (state == null)
        {
            error = "Состояние не найдено.";
            return false;
        }

        foreach (var s in States) s.IsStart = false;
        state.IsStart = true;
        error = null;
        return true;
    }

    public bool TryToggleHalting(string name, out string? error)
    {
        var state = States.FirstOrDefault(s => s.Name == name);
        if (state == null)
        {
            error = "Состояние не найдено.";
            return false;
        }

        state.IsHalting = !state.IsHalting;
        error = null;
        return true;
    }

    public bool TryAddOrUpdateRule(TransitionRule rule, bool deterministic, out string? error)
    {
        if (!Alphabet.Contains(rule.ReadSymbol) || !Alphabet.Contains(rule.WriteSymbol))
        {
            error = "Символ вне алфавита.";
            return false;
        }

        if (States.All(s => s.Name != rule.FromState) || States.All(s => s.Name != rule.ToState))
        {
            error = "Состояние не найдено в списке состояний.";
            return false;
        }

        var key = (rule.FromState, rule.ReadSymbol);
        if (deterministic && _rules.ContainsKey(key))
        {
            error = "Дублирующее правило для этого состояния и символа.";
            return false;
        }

        _rules[key] = rule;
        error = null;
        return true;
    }

    public bool RemoveRule(string fromState, char readSymbol)
    {
        return _rules.Remove((fromState, readSymbol));
    }

    public TransitionRule? FindRule(string fromState, char readSymbol)
    {
        _rules.TryGetValue((fromState, readSymbol), out var rule);
        return rule;
    }

    public IReadOnlyCollection<string> Validate()
    {
        var errors = new List<string>();
        if (!Alphabet.Validate(out var alphabetError))
        {
            errors.Add(alphabetError!);
        }

        if (States.Count == 0)
        {
            errors.Add("Добавьте хотя бы одно состояние.");
        }

        if (States.Count(s => s.IsStart) != 1)
        {
            errors.Add("Должно быть ровно одно начальное состояние.");
        }

        if (!States.Any(s => s.IsHalting))
        {
            errors.Add("Нужно хотя бы одно завершающее состояние.");
        }

        if (States.GroupBy(s => s.Name).Any(g => g.Count() > 1))
        {
            errors.Add("Имена состояний должны быть уникальны.");
        }

        foreach (var rule in _rules.Values)
        {
            if (!Alphabet.Contains(rule.ReadSymbol) || !Alphabet.Contains(rule.WriteSymbol))
            {
                errors.Add($"Правило {rule.FromState},{rule.ReadSymbol} содержит символ вне алфавита.");
            }
        }

        return errors;
    }
}
