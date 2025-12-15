using System.Collections.Generic;
using System.Linq;
using NfaVisualDebugger.Core.Automata;

namespace NfaVisualDebugger.Core.Algorithms
{
    public static class AutomatonValidator
    {
        public static List<string> Validate(Nfa nfa)
        {
            var errors = new List<string>();
            if (!nfa.StartStates().Any())
            {
                errors.Add("Нужно указать хотя бы одно стартовое состояние");
            }

            foreach (var t in nfa.Transitions)
            {
                if (t.FromStateId < 0 || t.FromStateId >= nfa.States.Count || t.ToStateId < 0 || t.ToStateId >= nfa.States.Count)
                {
                    errors.Add("Переход с некорректными конечными точками");
                }

                if (string.IsNullOrWhiteSpace(t.Label) && t.Label != Nfa.Epsilon)
                {
                    errors.Add("Пустые метки запрещены, используйте Оч для эпсилон-перехода");
                }
            }

            // optional unreachable states warning
            var reachable = new HashSet<int>(nfa.StartStates().Select(s => s.Id));
            var stack = new Stack<int>(reachable);
            while (stack.Count > 0)
            {
                var current = stack.Pop();
                foreach (var t in nfa.Transitions.Where(t => t.FromStateId == current))
                {
                    if (reachable.Add(t.ToStateId))
                    {
                        stack.Push(t.ToStateId);
                    }
                }
            }

            var unreachable = nfa.States.Where(s => !reachable.Contains(s.Id)).ToList();
            if (unreachable.Count > 0)
            {
                errors.Add($"Предупреждение: {unreachable.Count} недостижимых состояний");
            }

            return errors;
        }
    }
}
