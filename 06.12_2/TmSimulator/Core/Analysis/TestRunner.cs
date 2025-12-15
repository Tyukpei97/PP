using System;
using System.Collections.Generic;
using System.Linq;
using TmSimulator.Core.Machine;
using TmSimulator.Core.Simulation;

namespace TmSimulator.Core.Analysis;

public class TestRunner
{
    public MachineRunResult RunSingle(TmDefinition definition, string input, int stepLimit = 10000)
    {
        var runner = new TmRunner(definition)
        {
            StepLimit = stepLimit
        };

        if (!runner.Reset(input, out var error))
        {
            return new MachineRunResult
            {
                Input = input,
                Status = SimulationStatus.HaltedNoRule,
                Message = error ?? "Ошибка инициализации",
                Steps = 0,
                OutputTapeSnippet = string.Empty
            };
        }

        SimulationStatus status = SimulationStatus.Running;
        string message = string.Empty;

        while (status == SimulationStatus.Running)
        {
            var step = runner.Step();
            status = step.Status;
            message = step.Message;

            if (status != SimulationStatus.Running)
            {
                break;
            }
        }

        return new MachineRunResult
        {
            Input = input,
            Status = status,
            Steps = runner.StepCount,
            Message = message,
            OutputTapeSnippet = BuildTapeSnippet(runner, 20)
        };
    }

    public List<MachineRunResult> RunMany(TmDefinition definition, IEnumerable<string> inputs, int stepLimit = 10000)
    {
        return inputs.Select(input => RunSingle(definition, input, stepLimit)).ToList();
    }

    private static string BuildTapeSnippet(TmRunner runner, int radius)
    {
        if (runner.Tape.NonBlankCells.Count == 0)
        {
            return $"[{runner.HeadPosition}]";
        }

        var min = runner.Tape.NonBlankCells.Keys.Min();
        var max = runner.Tape.NonBlankCells.Keys.Max();
        min = Math.Min(min, runner.HeadPosition - radius);
        max = Math.Max(max, runner.HeadPosition + radius);

        var chars = new List<char>();
        for (var i = min; i <= max; i++)
        {
            chars.Add(runner.Tape.Read(i));
        }

        return $"[{runner.HeadPosition}] {new string(chars.ToArray())}";
    }
}
