using System.Collections.Generic;

public class OperatorPattern : Pattern
{
	private const int LEFT = 0;
	private const int OPERATOR = 2;
	private const int RIGHT = 4;

	// Pattern: ... [\n] $operator [\n] ...
	public OperatorPattern() : base
	(
		TokenType.OBJECT,
		TokenType.END | TokenType.OPTIONAL,
		TokenType.OPERATOR,
		TokenType.END | TokenType.OPTIONAL,
		TokenType.OBJECT
	)
	{ Priority = Parser.PRIORITY_ALL; }

	public override bool Passes(Context context, ParserState state, List<Token> tokens, int priority)
	{
		return tokens[OPERATOR].To<OperatorToken>().Operator.Priority == priority;
	}

	public override Node Build(Context context, ParserState state, List<Token> tokens)
	{
		return new OperatorNode(tokens[OPERATOR].To<OperatorToken>().Operator, tokens[OPERATOR].Position).SetOperands(
			Singleton.Parse(context, tokens[LEFT]),
			Singleton.Parse(context, tokens[RIGHT])
		);
	}
}