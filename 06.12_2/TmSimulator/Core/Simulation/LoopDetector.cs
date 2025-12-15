using System.Collections.Generic;

namespace TmSimulator.Core.Simulation;

public class LoopDetector
{
    private readonly HashSet<string> _seen = new();

    public void Reset()
    {
        _seen.Clear();
    }

    public bool IsRepeated(string hash)
    {
        if (_seen.Contains(hash))
            return true;
        _seen.Add(hash);
        return false;
    }
}
