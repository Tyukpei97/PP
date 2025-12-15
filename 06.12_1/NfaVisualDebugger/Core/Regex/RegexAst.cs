using System.Collections.Generic;

namespace NfaVisualDebugger.Core.Regex
{
    public abstract class RegexNode
    {
        public int Position { get; }

        protected RegexNode(int position)
        {
            Position = position;
        }
    }

    public class SymbolNode : RegexNode
    {
        public char Symbol { get; }
        public SymbolNode(char symbol, int position) : base(position) => Symbol = symbol;
    }

    public class CharacterClassNode : RegexNode
    {
        public IReadOnlyList<char> Symbols { get; }
        public CharacterClassNode(IReadOnlyList<char> symbols, int position) : base(position) => Symbols = symbols;
    }

    public class ConcatNode : RegexNode
    {
        public RegexNode Left { get; }
        public RegexNode Right { get; }
        public ConcatNode(RegexNode left, RegexNode right, int position) : base(position)
        {
            Left = left;
            Right = right;
        }
    }

    public class AlternationNode : RegexNode
    {
        public RegexNode Left { get; }
        public RegexNode Right { get; }
        public AlternationNode(RegexNode left, RegexNode right, int position) : base(position)
        {
            Left = left;
            Right = right;
        }
    }

    public class StarNode : RegexNode
    {
        public RegexNode Inner { get; }
        public StarNode(RegexNode inner, int position) : base(position) => Inner = inner;
    }

    public class PlusNode : RegexNode
    {
        public RegexNode Inner { get; }
        public PlusNode(RegexNode inner, int position) : base(position) => Inner = inner;
    }

    public class OptionalNode : RegexNode
    {
        public RegexNode Inner { get; }
        public OptionalNode(RegexNode inner, int position) : base(position) => Inner = inner;
    }

    public class EpsilonNode : RegexNode
    {
        public EpsilonNode(int position) : base(position) { }
    }

    public class EmptyNode : RegexNode
    {
        public EmptyNode(int position) : base(position) { }
    }
}
