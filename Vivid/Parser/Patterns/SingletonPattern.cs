using System.Collections.Generic;
using System.Linq;

public class SingletonPattern : Pattern
{
	public const int PRIORITY = 0;

	// Pattern:
	// Identifier / Function
	public SingletonPattern() : base(TokenType.IDENTIFIER | TokenType.FUNCTION | TokenType.NUMBER | TokenType.STRING | TokenType.CONTENT) { }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, PatternState state, List<Token> tokens)
	{
		return true;
	}

	public override Node Build(Context context, PatternState state, List<Token> tokens)
	{
		return Singleton.Parse(context, tokens.First());
	}
}