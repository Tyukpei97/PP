using System.Collections.Generic;
using System.Linq;

namespace NfaVisualDebugger.Core.Regex
{
    public class RegexParser
    {
        private readonly List<RegexToken> _tokens;
        private int _index;

        private RegexToken Current => _tokens[_index];

        public RegexParser(string pattern)
        {
            _tokens = new RegexTokenizer(pattern).Tokenize().ToList();
        }

        public RegexNode Parse()
        {
            var expr = ParseExpression();
            if (expr == null)
            {
                return new EpsilonNode(0);
            }

            if (Current.Type != RegexTokenType.End)
            {
                throw new RegexParseException($"Неожиданный символ '{Current.Text}'", Current.Position);
            }

            return expr;
        }

        private RegexNode? ParseExpression()
        {
            var left = ParseConcatenation();
            while (Match(RegexTokenType.Pipe))
            {
                var right = ParseConcatenation();
                if (right == null)
                {
                    throw new RegexParseException("После '|' ожидается подвыражение", Current.Position);
                }
                left = left == null ? right : new AlternationNode(left, right, right.Position);
            }
            return left;
        }

        private RegexNode? ParseConcatenation()
        {
            var nodes = new List<RegexNode>();
            while (true)
            {
                var unary = ParseUnary();
                if (unary == null)
                {
                    break;
                }
                nodes.Add(unary);
            }

            if (nodes.Count == 0)
            {
                return null;
            }

            var result = nodes[0];
            for (int i = 1; i < nodes.Count; i++)
            {
                result = new ConcatNode(result, nodes[i], nodes[i].Position);
            }
            return result;
        }

        private RegexNode? ParseUnary()
        {
            var primary = ParsePrimary();
            if (primary == null)
            {
                return null;
            }

            while (true)
            {
                if (Match(RegexTokenType.Star))
                {
                    primary = new StarNode(primary, Current.Position);
                    continue;
                }
                if (Match(RegexTokenType.Plus))
                {
                    primary = new PlusNode(primary, Current.Position);
                    continue;
                }
                if (Match(RegexTokenType.Question))
                {
                    primary = new OptionalNode(primary, Current.Position);
                    continue;
                }
                break;
            }

            return primary;
        }

        private RegexNode? ParsePrimary()
        {
            if (Current.Type == RegexTokenType.Symbol)
            {
                var token = Current;
                Advance();
                return new SymbolNode(token.Text[0], token.Position);
            }

            if (Current.Type == RegexTokenType.CharacterClass)
            {
                var token = Current;
                Advance();
                return new CharacterClassNode(token.Text.ToCharArray(), token.Position);
            }

            if (Match(RegexTokenType.LParen))
            {
                var inner = ParseExpression();
                if (!Match(RegexTokenType.RParen))
                {
                    throw new RegexParseException("Нет закрывающей скобки )", Current.Position);
                }
                return inner ?? new EpsilonNode(Current.Position);
            }

            // signals to caller that concatenation should stop
            if (Current.Type == RegexTokenType.RParen || Current.Type == RegexTokenType.Pipe || Current.Type == RegexTokenType.End)
            {
                return null;
            }

            throw new RegexParseException($"Неожиданный символ '{Current.Text}'", Current.Position);
        }

        private bool Match(RegexTokenType type)
        {
            if (Current.Type == type)
            {
                Advance();
                return true;
            }
            return false;
        }

        private void Advance()
        {
            if (_index < _tokens.Count - 1)
            {
                _index++;
            }
        }
    }
}
