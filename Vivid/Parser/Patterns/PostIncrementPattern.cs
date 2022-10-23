using System.Collections.Generic;

class PostIncrementPattern : Pattern
{
	public const int OBJECT = 0;
	public const int OPERATOR = 1;

	// Pattern 1: $value ++
	// Pattern 2: $value --
	public PostIncrementPattern() : base
	(
		TokenType.DYNAMIC | TokenType.IDENTIFIER, TokenType.OPERATOR
	)
	{ Priority = 18; }

	public override bool Passes(Context context, ParserState state, List<Token> tokens, int priority)
	{
		return tokens[OPERATOR].To<OperatorToken>().Operator == Operators.INCREMENT || tokens[OPERATOR].To<OperatorToken>().Operator == Operators.DECREMENT;
	}

	public override Node Build(Context context, ParserState state, List<Token> tokens)
	{
		if (tokens[OPERATOR].To<OperatorToken>().Operator == Operators.INCREMENT) return new IncrementNode(Singleton.Parse(context, tokens[OBJECT]), tokens[OPERATOR].Position, true);
		return new DecrementNode(Singleton.Parse(context, tokens[OBJECT]), tokens[OPERATOR].Position, true);
	}
}