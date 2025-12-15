using NfaVisualDebugger.Core.Automata;
using NfaVisualDebugger.Core.Regex;

namespace NfaVisualDebugger.Core.Algorithms
{
    public class ThompsonBuilder
    {
        private record Fragment(int StartId, int EndId);

        public Nfa Build(RegexNode node)
        {
            var nfa = new Nfa();
            var fragment = BuildInternal(node, nfa);
            nfa.States[fragment.StartId].IsStart = true;
            nfa.States[fragment.EndId].IsAccept = true;
            return nfa;
        }

        private Fragment BuildInternal(RegexNode node, Nfa nfa)
        {
            switch (node)
            {
                case SymbolNode symbol:
                    var s = nfa.AddState($"С{nfa.States.Count}", false, false);
                    var e = nfa.AddState($"С{nfa.States.Count}", false, false);
                    nfa.AddTransition(s.Id, e.Id, symbol.Symbol.ToString());
                    return new Fragment(s.Id, e.Id);

                case CharacterClassNode cls:
                    var cs = nfa.AddState($"С{nfa.States.Count}", false, false);
                    var ce = nfa.AddState($"С{nfa.States.Count}", false, false);
                    foreach (var ch in cls.Symbols)
                    {
                        nfa.AddTransition(cs.Id, ce.Id, ch.ToString());
                    }
                    return new Fragment(cs.Id, ce.Id);

                case ConcatNode concat:
                    var left = BuildInternal(concat.Left, nfa);
                    var right = BuildInternal(concat.Right, nfa);
                    nfa.AddTransition(left.EndId, right.StartId, Nfa.Epsilon);
                    return new Fragment(left.StartId, right.EndId);

                case AlternationNode alt:
                    var start = nfa.AddState($"С{nfa.States.Count}");
                    var end = nfa.AddState($"С{nfa.States.Count}");
                    var l = BuildInternal(alt.Left, nfa);
                    var r = BuildInternal(alt.Right, nfa);
                    nfa.AddTransition(start.Id, l.StartId, Nfa.Epsilon);
                    nfa.AddTransition(start.Id, r.StartId, Nfa.Epsilon);
                    nfa.AddTransition(l.EndId, end.Id, Nfa.Epsilon);
                    nfa.AddTransition(r.EndId, end.Id, Nfa.Epsilon);
                    return new Fragment(start.Id, end.Id);

                case StarNode star:
                    var starStart = nfa.AddState($"С{nfa.States.Count}");
                    var starEnd = nfa.AddState($"С{nfa.States.Count}");
                    var inner = BuildInternal(star.Inner, nfa);
                    nfa.AddTransition(starStart.Id, inner.StartId, Nfa.Epsilon);
                    nfa.AddTransition(starStart.Id, starEnd.Id, Nfa.Epsilon);
                    nfa.AddTransition(inner.EndId, inner.StartId, Nfa.Epsilon);
                    nfa.AddTransition(inner.EndId, starEnd.Id, Nfa.Epsilon);
                    return new Fragment(starStart.Id, starEnd.Id);

                case PlusNode plus:
                    var plusStart = nfa.AddState($"С{nfa.States.Count}");
                    var plusEnd = nfa.AddState($"С{nfa.States.Count}");
                    var pInner = BuildInternal(plus.Inner, nfa);
                    nfa.AddTransition(plusStart.Id, pInner.StartId, Nfa.Epsilon);
                    nfa.AddTransition(pInner.EndId, pInner.StartId, Nfa.Epsilon);
                    nfa.AddTransition(pInner.EndId, plusEnd.Id, Nfa.Epsilon);
                    return new Fragment(plusStart.Id, plusEnd.Id);

                case OptionalNode opt:
                    var optStart = nfa.AddState($"С{nfa.States.Count}");
                    var optEnd = nfa.AddState($"С{nfa.States.Count}");
                    var optInner = BuildInternal(opt.Inner, nfa);
                    nfa.AddTransition(optStart.Id, optInner.StartId, Nfa.Epsilon);
                    nfa.AddTransition(optStart.Id, optEnd.Id, Nfa.Epsilon);
                    nfa.AddTransition(optInner.EndId, optEnd.Id, Nfa.Epsilon);
                    return new Fragment(optStart.Id, optEnd.Id);

                case EpsilonNode:
                case EmptyNode:
                    var es = nfa.AddState($"С{nfa.States.Count}");
                    var ee = nfa.AddState($"С{nfa.States.Count}");
                    nfa.AddTransition(es.Id, ee.Id, Nfa.Epsilon);
                    return new Fragment(es.Id, ee.Id);

                default:
                    throw new RegexParseException("Неизвестный узел регулярного выражения", node.Position);
            }
        }
    }
}
