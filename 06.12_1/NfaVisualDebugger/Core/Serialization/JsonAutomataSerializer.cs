using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using NfaVisualDebugger.Core.Automata;

namespace NfaVisualDebugger.Core.Serialization
{
    public class AutomatonDto
    {
        public List<NfaState> States { get; set; } = new();
        public List<NfaTransition> Transitions { get; set; } = new();
    }

    public class ProjectDto
    {
        public AutomatonDto? AutomatonA { get; set; }
        public AutomatonDto? AutomatonB { get; set; }
        public string? RegexTextA { get; set; }
        public string? RegexTextB { get; set; }
        public string? LastInput { get; set; }
    }

    public static class JsonAutomataSerializer
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public static void Save(string filePath, Nfa nfa)
        {
            var dto = new AutomatonDto
            {
                States = new List<NfaState>(nfa.States),
                Transitions = new List<NfaTransition>(nfa.Transitions)
            };
            var json = JsonSerializer.Serialize(dto, Options);
            File.WriteAllText(filePath, json);
        }

        public static Nfa Load(string filePath)
        {
            var json = File.ReadAllText(filePath);
            var dto = JsonSerializer.Deserialize<AutomatonDto>(json, Options) ?? new AutomatonDto();
            var nfa = new Nfa();
            foreach (var s in dto.States)
            {
                nfa.States.Add(new NfaState(s.Id, s.Name, s.IsStart, s.IsAccept, s.X, s.Y));
            }
            foreach (var t in dto.Transitions)
            {
                nfa.Transitions.Add(new NfaTransition(t.FromStateId, t.ToStateId, t.Label));
            }
            return nfa;
        }

        public static void SaveProject(string filePath, ProjectDto project)
        {
            var json = JsonSerializer.Serialize(project, Options);
            File.WriteAllText(filePath, json);
        }

        public static ProjectDto LoadProject(string filePath)
        {
            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<ProjectDto>(json, Options) ?? new ProjectDto();
        }
    }
}
