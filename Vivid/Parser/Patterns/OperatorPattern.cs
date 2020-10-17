using System;
using System.Collections.Generic;

public class OperatorPattern : Pattern
{
	private const int LEFT = 0;
	private const int OPERATOR = 2;
	private const int RIGHT = 4;

	// Pattern:
	// ... [\n] Operator [\n] ...
	public OperatorPattern() : base
	(
		  TokenType.OBJECT,
		  TokenType.END | TokenType.OPTIONAL,
		  TokenType.OPERATOR,
		  TokenType.END | TokenType.OPTIONAL,
		  TokenType.OBJECT
	)
	{ }

	public override int GetPriority(List<Token> tokens)
	{
		return tokens[OPERATOR].To<OperatorToken>().Operator.Priority;
	}

	public override bool Passes(Context context, PatternState state, List<Token> tokens)
	{
		return true;
	}

	public override Node Build(Context context, List<Token> tokens)
	{
		return new OperatorNode(tokens[OPERATOR].To<OperatorToken>().Operator).SetOperands(
			Singleton.Parse(context, tokens[LEFT]),
			Singleton.Parse(context, tokens[RIGHT])
		);
	}
}