using System.Collections.Generic;
using System.Linq;
using NfaVisualDebugger.Core.Automata;

namespace NfaVisualDebugger.Core.Algorithms
{
    public record CounterExampleResult(string Word, bool AcceptedByA, bool AcceptedByB);

    public static class CounterExampleBfs
    {
        public static CounterExampleResult? Find(Dfa a, Dfa b)
        {
            if (a.States.Count == 0 || b.States.Count == 0)
            {
                return null;
            }

            var startA = a.States.First(s => s.IsStart).Id;
            var startB = b.States.First(s => s.IsStart).Id;

            var alphabet = a.Alphabet().Union(b.Alphabet()).ToList();
            var queue = new Queue<(int A, int B, string Word)>();
            var visited = new HashSet<(int, int)>();

            queue.Enqueue((startA, startB, string.Empty));
            visited.Add((startA, startB));

            while (queue.Count > 0)
            {
                var (stateA, stateB, word) = queue.Dequeue();
                var acceptA = a.States.First(s => s.Id == stateA)?.IsAccept ?? false;
                var acceptB = b.States.First(s => s.Id == stateB)?.IsAccept ?? false;

                if (acceptA != acceptB)
                {
                    return new CounterExampleResult(word, acceptA, acceptB);
                }

                foreach (var symbol in alphabet)
                {
                    var nextA = Move(a, stateA, symbol);
                    var nextB = Move(b, stateB, symbol);
                    var nextWord = word + symbol;
                    var key = (nextA, nextB);
                    if (visited.Add(key))
                    {
                        queue.Enqueue((nextA, nextB, nextWord));
                    }
                }
            }

            return null;
        }

        private static int Move(Dfa dfa, int stateId, string symbol)
        {
            if (stateId < 0 || !dfa.Transitions.TryGetValue(stateId, out var trans))
            {
                return -1;
            }

            return trans.TryGetValue(symbol, out var to) ? to : -1;
        }
    }
}
