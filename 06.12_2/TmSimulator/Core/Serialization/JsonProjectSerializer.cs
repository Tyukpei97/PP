using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using TmSimulator.Core.Machine;

namespace TmSimulator.Core.Serialization;

public class JsonProjectSerializer
{
    public void Save(string path, TmDefinition definition, IEnumerable<string> testInputs, string? lastTapeInput)
    {
        var model = new ProjectModel
        {
            Alphabet = definition.Alphabet.Symbols.ToList(),
            Blank = definition.Alphabet.BlankSymbol,
            States = definition.States.Select(s => new StateModel
            {
                Name = s.Name,
                IsStart = s.IsStart,
                IsHalting = s.IsHalting,
                X = s.X,
                Y = s.Y
            }).ToList(),
            Transitions = definition.Rules.Select(r => new TransitionModel
            {
                From = r.FromState,
                To = r.ToState,
                Read = r.ReadSymbol,
                Write = r.WriteSymbol,
                Move = r.Move switch
                {
                    Direction.Left => "L",
                    Direction.Right => "R",
                    _ => "S"
                }
            }).ToList(),
            StartState = definition.GetStartState()?.Name,
            HaltingStates = definition.GetHaltingStates().Select(s => s.Name).ToList(),
            TestInputs = testInputs.ToList(),
            LastTapeInput = lastTapeInput
        };

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        var json = JsonSerializer.Serialize(model, options);
        File.WriteAllText(path, json);
    }

    public bool TryLoad(string path, out TmDefinition? definition, out List<string> testInputs, out string? lastTapeInput, out string? error)
    {
        definition = null;
        testInputs = new List<string>();
        lastTapeInput = null;

        if (!File.Exists(path))
        {
            error = "Файл не найден.";
            return false;
        }

        try
        {
            var json = File.ReadAllText(path);
            var model = JsonSerializer.Deserialize<ProjectModel>(json);
            if (model == null)
            {
                error = "Не удалось прочитать проект.";
                return false;
            }

            var alphabet = model.Alphabet.Any()
                ? new Alphabet(model.Alphabet, model.Blank)
                : new Alphabet(new[] { model.Blank }, model.Blank);

            definition = new TmDefinition(alphabet);

            foreach (var s in model.States)
            {
                definition.AddState(s.Name, out _, s.IsStart, s.IsHalting, s.X, s.Y);
            }

            foreach (var t in model.Transitions)
            {
                var move = t.Move.ToUpperInvariant() switch
                {
                    "L" => Direction.Left,
                    "R" => Direction.Right,
                    _ => Direction.Stay
                };

                definition.TryAddOrUpdateRule(new TransitionRule(t.From, t.Read, t.To, t.Write, move), true, out _);
            }

            if (model.StartState != null)
            {
                definition.TrySetStartState(model.StartState, out _);
            }

            foreach (var h in model.HaltingStates)
            {
                var state = definition.States.FirstOrDefault(s => s.Name == h);
                if (state != null) state.IsHalting = true;
            }

            testInputs = model.TestInputs ?? new List<string>();
            lastTapeInput = model.LastTapeInput;
            error = null;
            return true;
        }
        catch (Exception ex)
        {
            error = $"Ошибка чтения JSON: {ex.Message}";
            return false;
        }
    }
}
