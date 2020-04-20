
using System.Collections.Generic;

public class UnarySignPattern : Pattern
{
    public const int PRIORITY = 17;

    public const int OPERATOR = 0;
    public const int SIGN = 1;
    public const int OBJECT = 2;

    /// Example:
    /// a = -x
    public UnarySignPattern() : base
    (
        TokenType.OPERATOR | TokenType.OPTIONAL,
        TokenType.OPERATOR,
        TokenType.OBJECT
    ) {}

    public override bool Passes(Context context, List<Token> tokens)
    {
        var sign = ((OperatorToken)tokens[SIGN]).Operator;
        return (sign == Operators.ADD || sign == Operators.SUBTRACT) && tokens[OPERATOR].Type != TokenType.NONE;
    }

    public override Node? Build(Context context, List<Token> tokens)
    {
        var target = Singleton.Parse(context, tokens[OBJECT]);
        var sign = ((OperatorToken)tokens[SIGN]).Operator;

        if (target is NumberNode number)
        {
            if (sign == Operators.SUBTRACT)
            {
                number.Negate();
            }

            return number;
        }

        if (sign == Operators.SUBTRACT)
        {
            return new NegateNode(target);
        }

        return target;
    }

    public override int GetPriority(List<Token> tokens)
    {
        return PRIORITY;
    }

    public override int GetStart()
    {
        return SIGN;
    }
}