using System.Collections.Generic;
using System.Linq;
using NfaVisualDebugger.Core.Automata;

namespace NfaVisualDebugger.Core.Algorithms
{
    public record SimulationStep(int Index, string? ConsumedSymbol, HashSet<int> ActiveStates, List<NfaTransition> HighlightedTransitions);
    public record SimulationResult(bool Accepted, List<SimulationStep> Steps);

    public static class NfaSimulator
    {
        public static SimulationResult Run(Nfa nfa, string input)
        {
            var steps = new List<SimulationStep>();
            var active = EpsilonClosure.Of(nfa, nfa.StartStates().Select(s => s.Id));
            steps.Add(new SimulationStep(0, null, active, new List<NfaTransition>()));

            var index = 0;
            foreach (var ch in input)
            {
                var used = new List<NfaTransition>();
                var moveTargets = new HashSet<int>();
                foreach (var state in active)
                {
                    foreach (var t in nfa.Transitions.Where(t => t.FromStateId == state && t.Label == ch.ToString()))
                    {
                        moveTargets.Add(t.ToStateId);
                        used.Add(t);
                    }
                }

                var closure = EpsilonClosure.Of(nfa, moveTargets);
                index++;
                steps.Add(new SimulationStep(index, ch.ToString(), closure, used));
                active = closure;
            }

            var accepted = active.Any(id => nfa.States[id].IsAccept);
            return new SimulationResult(accepted, steps);
        }
    }
}
