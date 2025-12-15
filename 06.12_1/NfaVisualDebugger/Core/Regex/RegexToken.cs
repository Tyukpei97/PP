namespace NfaVisualDebugger.Core.Regex
{
    public enum RegexTokenType
    {
        Symbol,
        CharacterClass,
        Pipe,
        Star,
        Plus,
        Question,
        LParen,
        RParen,
        LBracket,
        RBracket,
        End
    }

    public record RegexToken(RegexTokenType Type, string Text, int Position);
}
