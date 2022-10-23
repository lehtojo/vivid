using System.Collections.Generic;

class PreIncrementPattern : Pattern
{
	public const int OPERATOR = 0;
	public const int OBJECT = 1;

	// Pattern 1: ++ $value
	// Pattern 2: -- $value
	public PreIncrementPattern() : base
	(
		TokenType.OPERATOR, TokenType.DYNAMIC | TokenType.IDENTIFIER
	)
	{ Priority = 18; }

	public override bool Passes(Context context, ParserState state, List<Token> tokens, int priority)
	{
		return tokens[OPERATOR].To<OperatorToken>().Operator == Operators.INCREMENT || tokens[OPERATOR].To<OperatorToken>().Operator == Operators.DECREMENT;
	}

	public override Node Build(Context context, ParserState state, List<Token> tokens)
	{
		if (tokens[OPERATOR].To<OperatorToken>().Operator == Operators.INCREMENT) return new IncrementNode(Singleton.Parse(context, tokens[OBJECT]), tokens[OPERATOR].Position);
		return new DecrementNode(Singleton.Parse(context, tokens[OBJECT]), tokens[OPERATOR].Position);
	}
}