using System.Collections.Generic;
using System.Linq;
using NfaVisualDebugger.Core.Automata;

namespace NfaVisualDebugger.Core.Algorithms
{
    public static class SubsetConstruction
    {
        public static Dfa Build(Nfa nfa, int stateLimit, out bool truncated)
        {
            truncated = false;
            var dfa = new Dfa();
            var alphabet = nfa.Alphabet().ToList();
            var startStates = nfa.StartStates().Select(s => s.Id);
            var startClosure = EpsilonClosure.Of(nfa, startStates);

            var queue = new Queue<HashSet<int>>();
            var map = new Dictionary<string, int>();

            int AddState(HashSet<int> set, bool isStart)
            {
                var key = MakeKey(set);
                if (map.TryGetValue(key, out var id))
                {
                    return id;
                }

                id = map.Count;
                var isAccept = set.Any(id => nfa.States[id].IsAccept);
                dfa.AddState(new DfaState(id, set, isStart, isAccept));
                map[key] = id;
                queue.Enqueue(set);
                return id;
            }

            AddState(startClosure, true);

            while (queue.Count > 0)
            {
                var currentSet = queue.Dequeue();
                var currentId = map[MakeKey(currentSet)];

                foreach (var symbol in alphabet)
                {
                    var move = Move(nfa, currentSet, symbol);
                    if (move.Count == 0)
                    {
                        continue;
                    }

                    var closure = EpsilonClosure.Of(nfa, move);
                    var toId = AddState(closure, false);
                    dfa.AddTransition(currentId, symbol, toId);
                }

                if (dfa.States.Count > stateLimit)
                {
                    truncated = true;
                    break;
                }
            }

            return dfa;
        }

        private static HashSet<int> Move(Nfa nfa, HashSet<int> states, string symbol)
        {
            var result = new HashSet<int>();
            foreach (var s in states)
            {
                foreach (var t in nfa.Transitions.Where(t => t.FromStateId == s && t.Label == symbol))
                {
                    result.Add(t.ToStateId);
                }
            }
            return result;
        }

        private static string MakeKey(IEnumerable<int> set) => string.Join(",", set.OrderBy(x => x));
    }
}
