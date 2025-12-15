using System.Collections.Generic;
using System.Linq;

namespace TmSimulator.Core.Machine;

public class Alphabet
{
    public HashSet<char> Symbols { get; }
    public char BlankSymbol { get; private set; }

    public Alphabet(IEnumerable<char> symbols, char blankSymbol = '_')
    {
        Symbols = new HashSet<char>(symbols);
        if (!Symbols.Contains(blankSymbol))
        {
            Symbols.Add(blankSymbol);
        }
        BlankSymbol = blankSymbol;
    }

    public bool Validate(out string? error)
    {
        if (Symbols.Count == 0)
        {
            error = "Алфавит не может быть пустым.";
            return false;
        }
        if (!Symbols.Contains(BlankSymbol))
        {
            error = "Пробел должен входить в алфавит.";
            return false;
        }
        error = null;
        return true;
    }

    public void SetBlankSymbol(char symbol)
    {
        Symbols.Add(symbol);
        BlankSymbol = symbol;
    }

    public bool Contains(char symbol) => Symbols.Contains(symbol);

    public override string ToString() => string.Join(", ", Symbols.Select(c => c.ToString()));
}
