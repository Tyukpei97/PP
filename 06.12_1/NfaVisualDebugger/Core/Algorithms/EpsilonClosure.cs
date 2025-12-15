using System.Collections.Generic;
using System.Linq;
using NfaVisualDebugger.Core.Automata;

namespace NfaVisualDebugger.Core.Algorithms
{
    public static class EpsilonClosure
    {
        public static HashSet<int> Of(Nfa nfa, IEnumerable<int> states)
        {
            var closure = new HashSet<int>(states);
            var stack = new Stack<int>(states);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                foreach (var t in nfa.Transitions.Where(t => t.FromStateId == current && t.Label == Nfa.Epsilon))
                {
                    if (closure.Add(t.ToStateId))
                    {
                        stack.Push(t.ToStateId);
                    }
                }
            }

            return closure;
        }
    }
}
