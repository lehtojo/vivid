using System.Collections.Generic;

public class UnarySignPattern : Pattern
{
	public const int SIGN = 0;
	public const int OBJECT = 1;

	// Pattern 1: - $value
	// Pattern 2: + $value
	public UnarySignPattern() : base
	(
		TokenType.OPERATOR,
		TokenType.OBJECT
	)
	{ Priority = 18; }

	public override bool Passes(Context context, ParserState state, List<Token> tokens, int priority)
	{
		var sign = tokens[SIGN].To<OperatorToken>().Operator;
		if (sign != Operators.ADD && sign != Operators.SUBTRACT) return false;

		return state.Start == 0 || state.All[state.Start - 1].Is(TokenType.OPERATOR, TokenType.KEYWORD);
	}

	public override Node? Build(Context context, ParserState state, List<Token> tokens)
	{
		var value = Singleton.Parse(context, tokens[OBJECT]);
		var sign = tokens[SIGN].To<OperatorToken>().Operator;

		if (value.Instance == NodeType.NUMBER)
		{
			if (sign == Operators.SUBTRACT) value.To<NumberNode>().Negate();
			return value.To<NumberNode>();
		}

		return sign == Operators.SUBTRACT ? new NegateNode(value, tokens[SIGN].Position) : value;
	}
}