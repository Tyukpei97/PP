using System;
using System.Collections.Generic;

namespace NfaVisualDebugger.Core.Regex
{
    public class RegexTokenizer
    {
        private readonly string _pattern;

        public RegexTokenizer(string pattern)
        {
            _pattern = pattern ?? string.Empty;
        }

        public IEnumerable<RegexToken> Tokenize()
        {
            var i = 0;
            while (i < _pattern.Length)
            {
                var ch = _pattern[i];
                var pos = i;
                switch (ch)
                {
                    case '|':
                        yield return new RegexToken(RegexTokenType.Pipe, "|", pos);
                        i++;
                        break;
                    case '*':
                        yield return new RegexToken(RegexTokenType.Star, "*", pos);
                        i++;
                        break;
                    case '+':
                        yield return new RegexToken(RegexTokenType.Plus, "+", pos);
                        i++;
                        break;
                    case '?':
                        yield return new RegexToken(RegexTokenType.Question, "?", pos);
                        i++;
                        break;
                    case '(':
                        yield return new RegexToken(RegexTokenType.LParen, "(", pos);
                        i++;
                        break;
                    case ')':
                        yield return new RegexToken(RegexTokenType.RParen, ")", pos);
                        i++;
                        break;
                    case '[':
                        var classText = ReadClass(ref i);
                        yield return new RegexToken(RegexTokenType.CharacterClass, classText, pos);
                        break;
                    case '\\':
                        if (i + 1 >= _pattern.Length)
                        {
                            throw new RegexParseException("Ожидался символ после обратного слэша", pos);
                        }
                        yield return new RegexToken(RegexTokenType.Symbol, _pattern[i + 1].ToString(), pos);
                        i += 2;
                        break;
                    default:
                        yield return new RegexToken(RegexTokenType.Symbol, ch.ToString(), pos);
                        i++;
                        break;
                }
            }

            yield return new RegexToken(RegexTokenType.End, string.Empty, _pattern.Length);
        }

        private string ReadClass(ref int index)
        {
            var symbols = new List<char>();
            var start = index;
            index++; // skip '['
            var closed = false;
            while (index < _pattern.Length)
            {
                var ch = _pattern[index];
                if (ch == ']')
                {
                    closed = true;
                    index++; // consume ']'
                    break;
                }

                if (index + 2 < _pattern.Length && _pattern[index + 1] == '-' && _pattern[index + 2] != ']')
                {
                    var from = _pattern[index];
                    var to = _pattern[index + 2];
                    if (from > to)
                    {
                        throw new RegexParseException("Некорректный диапазон в классе символов", index);
                    }
                    for (var c = from; c <= to; c++)
                    {
                        symbols.Add(c);
                    }
                    index += 3;
                    continue;
                }

                symbols.Add(ch);
                index++;
            }

            if (!closed)
            {
                throw new RegexParseException("Нет закрывающей скобки ] в символьном классе", start);
            }

            return new string(symbols.ToArray());
        }
    }
}
