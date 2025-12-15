using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TmSimulator.Core.Machine;

namespace TmSimulator.Core.Parsing;

public class RuleParser
{
    private static readonly Regex RuleRegex = new(@"\(\s*(?<from>[^,]+)\s*,\s*(?<read>.)\s*\)\s*->\s*\(\s*(?<to>[^,]+)\s*,\s*(?<write>.)\s*,\s*(?<move>[LRS])\s*\)", RegexOptions.Compiled);

    public IReadOnlyCollection<TransitionRule> Parse(string text, out List<string> errors)
    {
        var rules = new List<TransitionRule>();
        errors = new List<string>();
        var lines = text.Replace("\r", "").Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            var raw = lines[i];
            if (string.IsNullOrWhiteSpace(raw))
                continue;

            var match = RuleRegex.Match(raw.Trim());
            if (!match.Success)
            {
                errors.Add($"Строка {i + 1}: не удалось разобрать правило.");
                continue;
            }

            var from = match.Groups["from"].Value.Trim();
            var read = match.Groups["read"].Value[0];
            var to = match.Groups["to"].Value.Trim();
            var write = match.Groups["write"].Value[0];
            var moveChar = match.Groups["move"].Value[0];
            var move = moveChar switch
            {
                'L' => Direction.Left,
                'R' => Direction.Right,
                'S' => Direction.Stay,
                _ => Direction.Stay
            };

            rules.Add(new TransitionRule(from, read, to, write, move));
        }

        return rules;
    }

    public IReadOnlyCollection<TransitionRule> ParseWithValidation(string text, TmDefinition definition, bool createMissingStates, out List<string> errors)
    {
        var parsed = Parse(text, out errors);
        foreach (var rule in parsed)
        {
            if (!definition.Alphabet.Contains(rule.ReadSymbol) || !definition.Alphabet.Contains(rule.WriteSymbol))
            {
                errors.Add($"Символ вне алфавита в правиле {rule.FromState},{rule.ReadSymbol}.");
            }

            if (definition.States.All(s => s.Name != rule.FromState))
            {
                if (createMissingStates)
                {
                    definition.AddState(rule.FromState, out _, false, false);
                }
                else
                {
                    errors.Add($"Неизвестное состояние: {rule.FromState}");
                }
            }

            if (definition.States.All(s => s.Name != rule.ToState))
            {
                if (createMissingStates)
                {
                    definition.AddState(rule.ToState, out _, false, false);
                }
                else
                {
                    errors.Add($"Неизвестное состояние: {rule.ToState}");
                }
            }
        }

        return parsed;
    }
}
