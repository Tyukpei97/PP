using System.Collections.Generic;
using System.Linq;

namespace NfaVisualDebugger.Core.Automata
{
    public class Nfa
    {
        public const string Epsilon = "Оч";

        public List<NfaState> States { get; } = new();
        public List<NfaTransition> Transitions { get; } = new();

        public NfaState AddState(string name, bool isStart = false, bool isAccept = false, double x = 0, double y = 0)
        {
            var state = new NfaState(States.Count, name, isStart, isAccept, x, y);
            States.Add(state);
            return state;
        }

        public void AddTransition(int fromId, int toId, string label)
        {
            Transitions.Add(new NfaTransition(fromId, toId, label));
        }

        public IEnumerable<string> Alphabet() =>
            Transitions.Select(t => t.Label)
                .Where(l => l != Epsilon && !string.IsNullOrWhiteSpace(l))
                .Distinct();

        public IEnumerable<NfaState> StartStates() => States.Where(s => s.IsStart);
        public IEnumerable<NfaState> AcceptStates() => States.Where(s => s.IsAccept);

        public Nfa Clone()
        {
            var clone = new Nfa();
            foreach (var s in States)
            {
                clone.States.Add(new NfaState(s.Id, s.Name, s.IsStart, s.IsAccept, s.X, s.Y));
            }

            foreach (var t in Transitions)
            {
                clone.Transitions.Add(new NfaTransition(t.FromStateId, t.ToStateId, t.Label));
            }

            return clone;
        }
    }
}
