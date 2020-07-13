using System.Collections.Generic;

public class SingletonPattern : Pattern
{
	public const int PRIORITY = 1;

	// Pattern:
	// Identifier / Function
	public SingletonPattern() : base(TokenType.IDENTIFIER | TokenType.FUNCTION | TokenType.NUMBER | TokenType.STRING | TokenType.CONTENT) { }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, List<Token> tokens)
	{
		return true;
	}

	public override Node Build(Context context, List<Token> tokens)
	{
		return Singleton.Parse(context, tokens[0]);
	}
}