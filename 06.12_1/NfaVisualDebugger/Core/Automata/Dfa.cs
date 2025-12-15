using System.Collections.Generic;
using System.Linq;

namespace NfaVisualDebugger.Core.Automata
{
    public class DfaState
    {
        public int Id { get; }
        public bool IsStart { get; set; }
        public bool IsAccept { get; set; }
        public HashSet<int> SourceNfaStates { get; }

        public DfaState(int id, IEnumerable<int> sourceStates, bool isStart, bool isAccept)
        {
            Id = id;
            SourceNfaStates = new HashSet<int>(sourceStates);
            IsStart = isStart;
            IsAccept = isAccept;
        }
    }

    public class Dfa
    {
        public List<DfaState> States { get; } = new();
        public Dictionary<int, Dictionary<string, int>> Transitions { get; } = new();

        public IEnumerable<string> Alphabet() =>
            Transitions.Values.SelectMany(t => t.Keys).Distinct();

        public void AddState(DfaState state)
        {
            States.Add(state);
            Transitions[state.Id] = new Dictionary<string, int>();
        }

        public void AddTransition(int fromId, string symbol, int toId)
        {
            if (!Transitions.TryGetValue(fromId, out var map))
            {
                map = new Dictionary<string, int>();
                Transitions[fromId] = map;
            }
            map[symbol] = toId;
        }

        public DfaState? GetState(int id) => States.FirstOrDefault(s => s.Id == id);
    }
}
