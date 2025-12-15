using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace MiniEmployeeDatabase;

public class EmployeeRepository
{
    private const string Header = "Id;Name;Position;Salary";
    private readonly string _filePath;

    public EmployeeRepository(string filePath)
    {
        _filePath = filePath;
    }

    public List<Employee> LoadEmployees()
    {
        var employees = new List<Employee>();

        if (!File.Exists(_filePath))
        {
            return employees;
        }

        foreach (var line in File.ReadLines(_filePath))
        {
            if (string.IsNullOrWhiteSpace(line) ||
                string.Equals(line.Trim(), Header, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var fields = SplitCsvLine(line);
            if (fields.Count < 4)
            {
                continue;
            }

            if (!Guid.TryParse(fields[0], out var id))
            {
                continue;
            }

            if (!decimal.TryParse(fields[3], NumberStyles.Number, CultureInfo.InvariantCulture, out var salary))
            {
                continue;
            }

            employees.Add(new Employee
            {
                Id = id,
                Name = fields[1],
                Position = fields[2],
                Salary = salary
            });
        }

        return employees;
    }

    public void SaveEmployees(IEnumerable<Employee> employees)
    {
        var lines = new List<string> { Header };

        foreach (var employee in employees)
        {
            lines.Add(string.Join(';', new[]
            {
                employee.Id.ToString(),
                Escape(employee.Name),
                Escape(employee.Position),
                employee.Salary.ToString(CultureInfo.InvariantCulture)
            }));
        }

        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllLines(_filePath, lines);
    }

    private static string Escape(string value)
    {
        if (value.IndexOfAny(new[] { ';', '"', '\n', '\r' }) >= 0)
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }

    private static List<string> SplitCsvLine(string line)
    {
        var result = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var ch = line[i];
            if (ch == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (ch == ';' && !inQuotes)
            {
                result.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(ch);
            }
        }

        result.Add(current.ToString());
        return result;
    }
}
