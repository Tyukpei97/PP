using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using TmSimulator.Core.Simulation;

namespace TmSimulator.Core.Analysis;

public class TraceExporter
{
    public void SaveAsText(string path, IEnumerable<TraceEntry> trace)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Шаг\tСостояние\tЧитает\tПишет\tДвижение\tПозиция");
        foreach (var entry in trace)
        {
            sb.AppendLine(string.Join('\t', entry.Step.ToString(CultureInfo.InvariantCulture),
                entry.State,
                entry.Read,
                entry.Write,
                entry.Move,
                entry.HeadPosition));
        }

        File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
    }
}
