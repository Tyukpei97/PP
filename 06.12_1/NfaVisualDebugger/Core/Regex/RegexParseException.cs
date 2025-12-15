using System;

namespace NfaVisualDebugger.Core.Regex
{
    public class RegexParseException : Exception
    {
        public int Position { get; }

        public RegexParseException(string message, int position) : base(message)
        {
            Position = position;
        }
    }
}
