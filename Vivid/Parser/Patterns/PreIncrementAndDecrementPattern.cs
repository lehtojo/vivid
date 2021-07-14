using System.Collections.Generic;

class PreIncrementAndDecrementPattern : Pattern
{
	public const int PRIORITY = 18;

	public const int OPERATOR = 0;
	public const int OBJECT = 1;

	// Pattern 1: ++ $value
	// Pattern 2: -- $value
	public PreIncrementAndDecrementPattern() : base
	(
		TokenType.OPERATOR, TokenType.DYNAMIC | TokenType.IDENTIFIER
	) { }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, PatternState state, List<Token> tokens)
	{
		return tokens[OPERATOR].To<OperatorToken>().Operator == Operators.INCREMENT || tokens[OPERATOR].To<OperatorToken>().Operator == Operators.DECREMENT;
	}

	public override Node Build(Context context, PatternState state, List<Token> tokens)
	{
		if (tokens[OPERATOR].To<OperatorToken>().Operator == Operators.INCREMENT) return new IncrementNode(Singleton.Parse(context, tokens[OBJECT]), tokens[OPERATOR].Position);
		return new DecrementNode(Singleton.Parse(context, tokens[OBJECT]), tokens[OPERATOR].Position);
	}
}