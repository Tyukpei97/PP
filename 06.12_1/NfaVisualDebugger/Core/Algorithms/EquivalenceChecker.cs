using System.Linq;
using NfaVisualDebugger.Core.Automata;

namespace NfaVisualDebugger.Core.Algorithms
{
    public record EquivalenceResult(bool Equivalent, string Message, CounterExampleResult? CounterExample, bool LimitReached);

    public static class EquivalenceChecker
    {
        public static EquivalenceResult Check(Nfa a, Nfa b, int dfaLimit = 2000)
        {
            if (!a.StartStates().Any() || !b.StartStates().Any())
            {
                return new EquivalenceResult(false, "У обоих автоматов должны быть стартовые состояния перед сравнением", null, false);
            }

            var dfaA = SubsetConstruction.Build(a, dfaLimit, out var truncA);
            var dfaB = SubsetConstruction.Build(b, dfaLimit, out var truncB);
            if (truncA || truncB)
            {
                return new EquivalenceResult(false, $"Порог {dfaLimit} состояний для DFA превышен, сравнение остановлено", null, true);
            }

            var minA = HopcroftMinimizer.Minimize(dfaA);
            var minB = HopcroftMinimizer.Minimize(dfaB);
            if (minA.States.Count > dfaLimit || minB.States.Count > dfaLimit)
            {
                return new EquivalenceResult(false, $"После минимизации количество состояний превысило {dfaLimit}", null, true);
            }

            var counter = CounterExampleBfs.Find(minA, minB);
            if (counter == null)
            {
                return new EquivalenceResult(true, "Автоматы эквивалентны", null, false);
            }

            var winner = counter.AcceptedByA ? "A" : "B";
            var loser = counter.AcceptedByA ? "B" : "A";
            var message = $"Автоматы различаются. Пример: '{counter.Word}' принимается автоматом {winner}, но отвергается автоматом {loser}.";
            return new EquivalenceResult(false, message, counter, false);
        }
    }
}
