using System.Collections.Generic;

public class NotPattern : Pattern
{
	public const int NOT = 0;
	public const int OBJECT = 1;

	public const int PRIORITY = 14;

	// Example: !/not $object
	public NotPattern() : base
	(
		TokenType.OPERATOR | TokenType.KEYWORD,
		TokenType.OBJECT
	) { }

	public override bool Passes(Context context, PatternState state, List<Token> tokens)
	{
		return tokens[NOT].Is(Operators.EXCLAMATION) || tokens[NOT].Is(Keywords.NOT);
	}

	public override Node? Build(Context context, PatternState state, List<Token> tokens)
	{
		return new NotNode(Singleton.Parse(context, tokens[OBJECT]), tokens[NOT].Position);
	}

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}
}