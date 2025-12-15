using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TmSimulator.Core.Machine;

namespace TmSimulator.Core.Simulation;

public class ConfigurationHasher
{
    public string Hash(string state, long headPosition, TapeModel tape, Alphabet alphabet)
    {
        var builder = new StringBuilder();
        builder.Append(state);
        builder.Append('|');
        builder.Append(headPosition);
        builder.Append('|');

        foreach (var cell in tape.NonBlankCells.OrderBy(c => c.Key))
        {
            builder.Append(cell.Key);
            builder.Append(':');
            builder.Append(cell.Value);
            builder.Append(',');
        }

        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(builder.ToString());
        var hash = sha.ComputeHash(bytes);
        return Convert.ToHexString(hash);
    }
}
