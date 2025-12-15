using System.Collections.Generic;
using System.Linq;
using TmSimulator.Core.Machine;
using TmSimulator.Core.Simulation;

namespace TmSimulator.Core.Analysis;

public class ComparisonEngine
{
    private readonly TestRunner _runner = new();

    public List<ComparisonResult> Compare(TmDefinition machineA, TmDefinition machineB, IEnumerable<string> inputs, int stepLimit = 10000)
    {
        var results = new List<ComparisonResult>();
        foreach (var input in inputs)
        {
            var a = _runner.RunSingle(machineA, input, stepLimit);
            var b = _runner.RunSingle(machineB, input, stepLimit);

            var verdict = BuildVerdict(a, b);

            results.Add(new ComparisonResult
            {
                Input = input,
                StatusA = a.Status,
                StatusB = b.Status,
                OutputA = a.OutputTapeSnippet,
                OutputB = b.OutputTapeSnippet,
                Verdict = verdict
            });
        }

        return results;
    }

    private static string BuildVerdict(MachineRunResult a, MachineRunResult b)
    {
        if (a.Status == b.Status && a.OutputTapeSnippet == b.OutputTapeSnippet)
            return "Совпадает на данном тесте";

        if (a.Status != b.Status)
            return "Различие: разный результат (завершение/ошибка/цикл)";

        return "Различие: разные выходные данные";
    }
}
