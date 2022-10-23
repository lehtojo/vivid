using System.Collections.Generic;
using System.Linq;

public class SingletonPattern : Pattern
{
	// Pattern: ...
	public SingletonPattern() : base(TokenType.IDENTIFIER | TokenType.FUNCTION | TokenType.NUMBER | TokenType.STRING | TokenType.PARENTHESIS)
	{
		Priority = 0;
		IsConsumable = false;
	}

	public override bool Passes(Context context, ParserState state, List<Token> tokens, int priority)
	{
		return true;
	}

	public override Node Build(Context context, ParserState state, List<Token> tokens)
	{
		return Singleton.Parse(context, tokens.First());
	}
}