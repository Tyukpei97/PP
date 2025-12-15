using System.Globalization;

namespace FunctionGraphingCalculator;

internal sealed class ExpressionEvaluator
{
    private readonly List<Token> _rpn;

    private ExpressionEvaluator(List<Token> rpn)
    {
        _rpn = rpn;
    }

    public static ExpressionEvaluator Parse(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            throw new ArgumentException("Expression cannot be empty.");
        }

        var tokens = Tokenize(expression);
        var rpn = ToRpn(tokens);
        return new ExpressionEvaluator(rpn);
    }

    public double Evaluate(double x)
    {
        var stack = new Stack<double>();
        foreach (var token in _rpn)
        {
            switch (token.Kind)
            {
                case TokenKind.Number:
                case TokenKind.Constant:
                    stack.Push(token.Value);
                    break;
                case TokenKind.Variable:
                    stack.Push(x);
                    break;
                case TokenKind.UnaryOperator:
                    if (stack.Count < 1)
                    {
                        throw new InvalidOperationException("Invalid expression.");
                    }
                    stack.Push(-stack.Pop());
                    break;
                case TokenKind.Operator:
                    if (stack.Count < 2)
                    {
                        throw new InvalidOperationException("Invalid expression.");
                    }
                    double b = stack.Pop();
                    double a = stack.Pop();
                    stack.Push(ApplyOperator(token.Symbol, a, b));
                    break;
                case TokenKind.Function:
                    if (stack.Count < 1)
                    {
                        throw new InvalidOperationException("Invalid expression.");
                    }
                    stack.Push(ApplyFunction(token.Symbol, stack.Pop()));
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported token '{token.Symbol}'.");
            }
        }

        if (stack.Count != 1)
        {
            throw new InvalidOperationException("Invalid expression.");
        }

        return stack.Pop();
    }

    private static List<Token> Tokenize(string expression)
    {
        var tokens = new List<Token>();
        int i = 0;
        TokenKind? prev = null;

        while (i < expression.Length)
        {
            char c = expression[i];
            if (char.IsWhiteSpace(c))
            {
                i++;
                continue;
            }

            if (char.IsDigit(c) || c == '.')
            {
                int start = i;
                i++;
                while (i < expression.Length)
                {
                    char ch = expression[i];
                    if (char.IsDigit(ch) || ch == '.')
                    {
                        i++;
                        continue;
                    }

                    if ((ch == 'e' || ch == 'E') && i + 1 < expression.Length)
                    {
                        if (char.IsDigit(expression[i + 1]) || expression[i + 1] == '+' || expression[i + 1] == '-')
                        {
                            i += 2;
                            while (i < expression.Length && char.IsDigit(expression[i]))
                            {
                                i++;
                            }
                            continue;
                        }
                    }

                    break;
                }

                string numberText = expression[start..i];
                if (!double.TryParse(numberText, NumberStyles.Float, CultureInfo.InvariantCulture, out double number))
                {
                    throw new FormatException($"Invalid number '{numberText}'.");
                }

                tokens.Add(new Token(TokenKind.Number, numberText, number));
                prev = TokenKind.Number;
                continue;
            }

            if (char.IsLetter(c))
            {
                int start = i;
                i++;
                while (i < expression.Length && (char.IsLetter(expression[i]) || char.IsDigit(expression[i])))
                {
                    i++;
                }

                string ident = expression[start..i];
                if (string.Equals(ident, "x", StringComparison.OrdinalIgnoreCase))
                {
                    tokens.Add(new Token(TokenKind.Variable, ident, 0));
                    prev = TokenKind.Variable;
                }
                else if (Constants.TryGetValue(ident, out double value))
                {
                    tokens.Add(new Token(TokenKind.Constant, ident, value));
                    prev = TokenKind.Constant;
                }
                else
                {
                    tokens.Add(new Token(TokenKind.Function, ident, 0));
                    prev = TokenKind.Function;
                }

                continue;
            }

            if ("+-*/^".Contains(c))
            {
                bool unary = c == '-' && (prev is null || prev is TokenKind.Operator or TokenKind.UnaryOperator or TokenKind.LeftParen or TokenKind.Function);
                tokens.Add(new Token(unary ? TokenKind.UnaryOperator : TokenKind.Operator, c.ToString(), 0));
                prev = unary ? TokenKind.UnaryOperator : TokenKind.Operator;
                i++;
                continue;
            }

            if (c == '(')
            {
                tokens.Add(new Token(TokenKind.LeftParen, "(", 0));
                prev = TokenKind.LeftParen;
                i++;
                continue;
            }

            if (c == ')')
            {
                tokens.Add(new Token(TokenKind.RightParen, ")", 0));
                prev = TokenKind.RightParen;
                i++;
                continue;
            }

            throw new FormatException($"Unexpected character '{c}'.");
        }

        return tokens;
    }

    private static List<Token> ToRpn(List<Token> tokens)
    {
        var output = new List<Token>();
        var stack = new Stack<Token>();

        foreach (var token in tokens)
        {
            switch (token.Kind)
            {
                case TokenKind.Number:
                case TokenKind.Variable:
                case TokenKind.Constant:
                    output.Add(token);
                    break;
                case TokenKind.Function:
                    stack.Push(token);
                    break;
                case TokenKind.Operator:
                case TokenKind.UnaryOperator:
                    while (stack.Count > 0 && IsOperator(stack.Peek()))
                    {
                        var top = stack.Peek();
                        if ((IsRightAssociative(token) && Precedence(token) < Precedence(top)) ||
                            (!IsRightAssociative(token) && Precedence(token) <= Precedence(top)))
                        {
                            output.Add(stack.Pop());
                        }
                        else
                        {
                            break;
                        }
                    }
                    stack.Push(token);
                    break;
                case TokenKind.LeftParen:
                    stack.Push(token);
                    break;
                case TokenKind.RightParen:
                    while (stack.Count > 0 && stack.Peek().Kind != TokenKind.LeftParen)
                    {
                        output.Add(stack.Pop());
                    }

                    if (stack.Count == 0)
                    {
                        throw new FormatException("Mismatched parentheses.");
                    }

                    stack.Pop(); // remove '('
                    if (stack.Count > 0 && stack.Peek().Kind == TokenKind.Function)
                    {
                        output.Add(stack.Pop());
                    }
                    break;
            }
        }

        while (stack.Count > 0)
        {
            var token = stack.Pop();
            if (token.Kind is TokenKind.LeftParen or TokenKind.RightParen)
            {
                throw new FormatException("Mismatched parentheses.");
            }

            output.Add(token);
        }

        return output;
    }

    private static double ApplyOperator(string symbol, double a, double b)
    {
        return symbol switch
        {
            "+" => a + b,
            "-" => a - b,
            "*" => a * b,
            "/" => b == 0 ? double.NaN : a / b,
            "^" => Math.Pow(a, b),
            _ => throw new InvalidOperationException($"Unknown operator '{symbol}'.")
        };
    }

    private static double ApplyFunction(string name, double value)
    {
        switch (name.ToLowerInvariant())
        {
            case "sin":
                return Math.Sin(value);
            case "cos":
                return Math.Cos(value);
            case "tan":
                return Math.Tan(value);
            case "sqrt":
                return value < 0 ? double.NaN : Math.Sqrt(value);
            case "log":
                return value <= 0 ? double.NaN : Math.Log(value);
            case "abs":
                return Math.Abs(value);
            default:
                throw new ArgumentException($"Unknown function '{name}'.");
        }
    }

    private static bool IsOperator(Token token) =>
        token.Kind is TokenKind.Operator or TokenKind.UnaryOperator;

    private static bool IsRightAssociative(Token token) =>
        token.Kind == TokenKind.UnaryOperator || (Operators.TryGetValue(token.Symbol, out var op) && op.RightAssociative);

    private static int Precedence(Token token) =>
        token.Kind == TokenKind.UnaryOperator ? 4 : Operators.TryGetValue(token.Symbol, out var op) ? op.Precedence : 0;

    private static readonly Dictionary<string, (int Precedence, bool RightAssociative)> Operators =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["+"] = (1, false),
            ["-"] = (1, false),
            ["*"] = (2, false),
            ["/"] = (2, false),
            ["^"] = (3, true)
        };

    private static readonly Dictionary<string, double> Constants = new(StringComparer.OrdinalIgnoreCase)
    {
        ["pi"] = Math.PI,
        ["e"] = Math.E
    };

    private enum TokenKind
    {
        Number,
        Variable,
        Constant,
        Operator,
        UnaryOperator,
        Function,
        LeftParen,
        RightParen
    }

    private readonly record struct Token(TokenKind Kind, string Symbol, double Value);
}
