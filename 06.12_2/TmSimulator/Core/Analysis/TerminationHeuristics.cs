using System.Collections.Generic;
using System.Linq;
using TmSimulator.Core.Machine;

namespace TmSimulator.Core.Analysis;

public class TerminationHeuristics
{
    public IReadOnlyCollection<string> GetHints(TmDefinition definition)
    {
        var hints = new List<string>();

        var moves = definition.Rules.Select(r => r.Move).ToList();
        if (moves.All(m => m == Direction.Right))
        {
            hints.Add("Предположение: головка всегда движется вправо — цикл маловероятен при конечном вводе.");
        }
        else if (moves.All(m => m == Direction.Left))
        {
            hints.Add("Предположение: головка всегда движется влево — проверьте, не упирается ли в пустую ленту.");
        }

        if (!definition.Rules.Any())
        {
            hints.Add("Правила отсутствуют — машина сразу останавливается.");
        }

        return hints;
    }
}
