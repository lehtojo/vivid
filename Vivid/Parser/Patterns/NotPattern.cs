using System.Collections.Generic;

public class NotPattern : Pattern
{
	public const int NOT = 0;
	public const int OBJECT = 1;

	// Pattern: !/not $object
	public NotPattern() : base
	(
		TokenType.OPERATOR | TokenType.KEYWORD,
		TokenType.OBJECT
	)
	{ Priority = 14; }

	public override bool Passes(Context context, ParserState state, List<Token> tokens, int priority)
	{
		return tokens[NOT].Is(Operators.EXCLAMATION) || tokens[NOT].Is(Keywords.NOT);
	}

	public override Node? Build(Context context, ParserState state, List<Token> tokens)
	{
		return new NotNode(Singleton.Parse(context, tokens[OBJECT]), tokens[NOT].Is(Operators.EXCLAMATION), tokens[NOT].Position);
	}
}