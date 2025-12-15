using System.Collections.Generic;
using System.Linq;
using NfaVisualDebugger.Core.Automata;

namespace NfaVisualDebugger.Core.Algorithms
{
    public static class HopcroftMinimizer
    {
        public static Dfa Minimize(Dfa dfa)
        {
            if (dfa.States.Count == 0)
            {
                return new Dfa();
            }

            var alphabet = dfa.Alphabet().ToList();
            var accepting = dfa.States.Where(s => s.IsAccept).Select(s => s.Id).ToHashSet();
            var nonAccepting = dfa.States.Where(s => !s.IsAccept).Select(s => s.Id).ToHashSet();

            var partitions = new List<HashSet<int>>();
            if (accepting.Count > 0)
                partitions.Add(accepting);
            if (nonAccepting.Count > 0)
                partitions.Add(nonAccepting);

            var work = new Queue<HashSet<int>>(partitions.Select(p => new HashSet<int>(p)));

            while (work.Count > 0)
            {
                var a = work.Dequeue();
                foreach (var symbol in alphabet)
                {
                    var x = Predecessors(dfa, a, symbol);
                    for (int i = 0; i < partitions.Count; i++)
                    {
                        var y = partitions[i];
                        var intersection = y.Intersect(x).ToHashSet();
                        if (intersection.Count == 0 || intersection.Count == y.Count)
                        {
                            continue;
                        }

                        var difference = y.Except(x).ToHashSet();
                        partitions[i] = intersection;
                        partitions.Insert(i + 1, difference);

                        // maintain work list
                        bool replaced = false;
                        var updatedWork = new Queue<HashSet<int>>();
                        while (work.Count > 0)
                        {
                            var w = work.Dequeue();
                            if (w.SetEquals(y))
                            {
                                updatedWork.Enqueue(intersection);
                                updatedWork.Enqueue(difference);
                                replaced = true;
                            }
                            else
                            {
                                updatedWork.Enqueue(w);
                            }
                        }

                        if (!replaced)
                        {
                            if (intersection.Count <= difference.Count)
                                work.Enqueue(intersection);
                            else
                                work.Enqueue(difference);
                        }

                        work = new Queue<HashSet<int>>(updatedWork);
                    }
                }
            }

            // build minimized DFA
            var minimized = new Dfa();
            var blockMap = new Dictionary<int, int>(); // original state -> minimized state id
            for (int i = 0; i < partitions.Count; i++)
            {
                var block = partitions[i];
                var sample = block.First();
                var sourceStates = dfa.States.First(s => s.Id == sample).SourceNfaStates;
                var isStart = block.Any(id => dfa.States.First(s => s.Id == id).IsStart);
                var isAccept = block.Any(id => dfa.States.First(s => s.Id == id).IsAccept);
                minimized.AddState(new DfaState(i, sourceStates, isStart, isAccept));
                foreach (var stateId in block)
                {
                    blockMap[stateId] = i;
                }
            }

            foreach (var block in partitions)
            {
                var representative = block.First();
                var fromId = blockMap[representative];
                if (!dfa.Transitions.TryGetValue(representative, out var trans))
                {
                    continue;
                }
                foreach (var (symbol, target) in trans)
                {
                    if (blockMap.TryGetValue(target, out var toId))
                    {
                        minimized.AddTransition(fromId, symbol, toId);
                    }
                }
            }

            return minimized;
        }

        private static HashSet<int> Predecessors(Dfa dfa, HashSet<int> states, string symbol)
        {
            var result = new HashSet<int>();
            foreach (var (from, transitions) in dfa.Transitions)
            {
                if (transitions.TryGetValue(symbol, out var to) && states.Contains(to))
                {
                    result.Add(from);
                }
            }
            return result;
        }
    }
}
