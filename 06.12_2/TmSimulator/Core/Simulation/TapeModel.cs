using System.Collections.Generic;
using System.Linq;
using TmSimulator.Core.Machine;

namespace TmSimulator.Core.Simulation;

public class TapeModel
{
    private readonly Dictionary<long, char> _cells = new();
    private Alphabet _alphabet;

    public TapeModel(Alphabet alphabet)
    {
        _alphabet = alphabet;
    }

    public void SetAlphabet(Alphabet alphabet)
    {
        _alphabet = alphabet;
        var toRemove = _cells.Where(kv => !alphabet.Contains(kv.Value)).Select(kv => kv.Key).ToList();
        foreach (var key in toRemove)
        {
            _cells.Remove(key);
        }
    }

    public char Read(long position)
    {
        return _cells.TryGetValue(position, out var value) ? value : _alphabet.BlankSymbol;
    }

    public void Write(long position, char symbol)
    {
        if (symbol == _alphabet.BlankSymbol)
        {
            _cells.Remove(position);
        }
        else
        {
            _cells[position] = symbol;
        }
    }

    public IReadOnlyDictionary<long, char> NonBlankCells => _cells;

    public void Clear()
    {
        _cells.Clear();
    }

    public bool TrySetInput(string input, out string? error)
    {
        _cells.Clear();
        for (int i = 0; i < input.Length; i++)
        {
            var ch = input[i];
            if (!_alphabet.Contains(ch))
            {
                error = $"Символ '{ch}' не входит в алфавит.";
                _cells.Clear();
                return false;
            }
            if (ch != _alphabet.BlankSymbol)
            {
                _cells[i] = ch;
            }
        }

        error = null;
        return true;
    }

    public IEnumerable<(long position, char symbol)> Window(long center, int radius)
    {
        for (var i = center - radius; i <= center + radius; i++)
        {
            yield return (i, Read(i));
        }
    }
}
